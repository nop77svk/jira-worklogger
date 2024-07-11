namespace jwl.jira;

using System.Globalization;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using jwl.infra;
using jwl.jira.api.rest.request;
using jwl.jira.Exceptions;
using jwl.jira.Flavours;
using jwl.wadl;

// https://interconcept.atlassian.net/wiki/spaces/ICTIME/pages/31686672/API
public class JiraWithICTimePluginApi
    : IJiraClient
{
    public string UserName { get; }
    public api.rest.common.JiraUserInfo UserInfo => _vanillaJiraApi.UserInfo;

    public Lazy<Dictionary<string, wadl.ComposedWadlMethodDefinition>> Endpoints =>
        new Lazy<Dictionary<string, ComposedWadlMethodDefinition>>(() => this.GetWADL().Result
            .AsComposedWadlMethodDefinitionEnumerable()
            .Where(res => !string.IsNullOrEmpty(res.Id))
            // 2do! a nasty hack! remake to something more clever!
            .Where(res => res.Id is GetActivityTypesForProjectMethodName or CreateWorkLogMethodName)
            .ToDictionary(res => res.Id ?? string.Empty)
        );

    public const string CreateWorkLogMethodName = "createWorklog";
    public wadl.ComposedWadlMethodDefinition CreateWorkLogMethodDefinition => Endpoints.Value[CreateWorkLogMethodName];

    public const string GetActivityTypesForProjectMethodName = "getActivityTypesForProject";
    public wadl.ComposedWadlMethodDefinition GetActivityTypesForProjectMethodDefinition => Endpoints.Value[GetActivityTypesForProjectMethodName];

    private readonly HttpClient _httpClient;
    private readonly FlavourICTimeOptions _flavourOptions;
    private readonly FlavourICTimeOptions _defaultFlavourOptions = new FlavourICTimeOptions();
    private readonly VanillaJiraClient _vanillaJiraApi;

    public JiraWithICTimePluginApi(HttpClient httpClient, string userName, VanillaJiraClient vanillaJiraClient, FlavourICTimeOptions? flavourOptions)
    {
        _httpClient = httpClient;
        UserName = userName;
        _flavourOptions = flavourOptions ?? _defaultFlavourOptions;
        _vanillaJiraApi = vanillaJiraClient;
    }

    public async Task<WorkLogType[]> GetAvailableActivities(string issueKey)
    {
        var result = await GetAvailableActivities(new string[] { issueKey });
        return result[issueKey];
    }

    public async Task<Dictionary<string, WorkLogType[]>> GetAvailableActivities(IEnumerable<string> issueKeys)
    {
        IEnumerable<JiraIssueKey> issueKeysParsed = issueKeys
            .Select(issueKey => new JiraIssueKey(issueKey))
            .ToArray();

        IEqualityComparer<JiraIssueKey> projectComparer = new JiraIssueKey.ProjectOnlyEqualityComparer();
        IEnumerable<JiraIssueKey> distinctProjects = issueKeysParsed
            .Distinct(projectComparer);

        ComposedWadlMethodDefinition endPoint = GetActivityTypesForProjectMethodDefinition;

        HashSet<string> missingParameters = endPoint.Parameters
            .Concat(endPoint.Request?.Parameters ?? Array.Empty<WadlParameter>())
            .Where(par => !string.IsNullOrEmpty(par.Name))
            .Select(par => par.Name ?? string.Empty)
            .ToHashSet();

        // define
        IEnumerable<KeyValuePair<JiraIssueKey, string>> uris = distinctProjects
            .Select(issueKey => new KeyValuePair<JiraIssueKey, string>(
                issueKey,
                _flavourOptions.PluginBaseUri.Trim('/')
                    + "/"
                    + endPoint.ResourcePath
                        .Replace("{issueKey}", issueKey.ToString())
                        .Trim('/')
            ));

        missingParameters.Remove("issueKey");

        // check
        HttpMethod expectedMethod = HttpMethod.Get;
        bool isCorrectHttpMethod = endPoint.HttpMethod == expectedMethod;
        if (!isCorrectHttpMethod)
            throw new InvalidOperationException($"Method {endPoint.Id} at resource path {endPoint.ResourcePath} executes via ${endPoint.HttpMethod} method (${expectedMethod.ToString().ToUpperInvariant()} expected)");

        if (missingParameters.Any())
            throw new ArgumentException($"Missing assignment of {string.Join(',', missingParameters)} in the call of {endPoint.Id} at resource path {endPoint.ResourcePath}");

        bool providesJsonResponses = endPoint.Response?.Representations?
            .Any(repr => repr.MediaType == WadlRepresentation.MediaTypes.Json) ?? false;

        if (!providesJsonResponses)
            throw new InvalidOperationException($"Method {endPoint.Id} at resource path {endPoint.ResourcePath} does not respond in JSON");

        // execute
        KeyValuePair<JiraIssueKey, Task<api.rest.response.ICTimeActivityDefinition[]>>[] responseTaks = uris
            .Select(uri => new KeyValuePair<JiraIssueKey, Task<api.rest.response.ICTimeActivityDefinition[]>>(uri.Key, _httpClient.GetAsJsonAsync<api.rest.response.ICTimeActivityDefinition[]>(uri.Value)))
            .ToArray();

        await Task.WhenAll(responseTaks.Select(x => x.Value));

        Dictionary<string, WorkLogType[]> result = responseTaks
            .Join(
                inner: issueKeysParsed,
                outerKeySelector: outer => outer.Key,
                innerKeySelector: inner => inner,
                resultSelector: (outer, inner) => new KeyValuePair<string, WorkLogType[]>(
                    outer.Key.ToString(),
                    outer.Value.Result
                        .Select((def, ix) => new WorkLogType(
                            Key: def.Id.ToString(),
                            Value: def.Name,
                            Sequence: ix
                        ))
                        .ToArray()
                ),
                comparer: projectComparer
            )
            .ToDictionary(
                keySelector: x => x.Key,
                elementSelector: x => x.Value
            );

        return result;
    }

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, string issueKey)
    {
        return await _vanillaJiraApi.GetIssueWorkLogs(from, to, issueKey);
    }

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        return await _vanillaJiraApi.GetIssueWorkLogs(from, to, issueKeys);
    }

    public async Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        ComposedWadlMethodDefinition endPoint = CreateWorkLogMethodDefinition;

        HashSet<string> missingParameters = endPoint.Parameters
            .Concat(endPoint.Request?.Representations?.First().Parameters ?? Array.Empty<WadlParameter>()) // 2do! not just the first representation, but the correct representation
            .Where(par => !string.IsNullOrEmpty(par.Name))
            .Select(par => par.Name ?? string.Empty)
            .ToHashSet();

        // define
        string uri = this._flavourOptions.PluginBaseUri.Trim('/') + "/" + endPoint.ResourcePath
            .Replace("{issueKey}", issueKey)
            .Trim('/');
        missingParameters.Remove("issueKey");

        string activityArg;
        try
        {
            activityArg = _flavourOptions.ActivityMap == null || !_flavourOptions.ActivityMap.Any() || string.IsNullOrEmpty(activity)
                ? activity ?? string.Empty
                : _flavourOptions.ActivityMap[activity];
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidDataException($"Failed to remap activity \"{activity}\" based on flavour-specific activity map");
        }

        string dateArg = day
            .ToDateTime(TimeOnly.MinValue)
            .ToString(_flavourOptions.DateFormat, CultureInfo.InvariantCulture);
        string logWorkOptionArg = ICTimeAddWorklogByIssueKey.LogWorkOption.Summary
            .ToString()
            .ToLowerFirstChar();
        string timeLoggedArg = TimeSpan.FromSeconds(timeSpentSeconds)
            .ToString(_flavourOptions.TimeSpanFormat, CultureInfo.InvariantCulture);
        string chargedArg = true
            .ToString()
            .ToLowerInvariant();
        Dictionary<string, string> args = new ()
        {
            ["date"] = dateArg,
            ["logWorkOption"] = logWorkOptionArg,
            ["comment"] = comment ?? string.Empty,
            ["timeLogged"] = timeLoggedArg,
            ["activity"] = activityArg,
            ["user"] = this.UserName,
            ["charged"] = chargedArg
        };
        missingParameters.RemoveWhere(elm => args.ContainsKey(elm));

        HashSet<string> ignoreParameters = new HashSet<string>()
        {
            "startTime",
            "endTime",
            "remainingEstimate",
            "timeSpentCorrected",
            "noChargeInfo"
        };
        missingParameters.RemoveWhere(elm => ignoreParameters.Contains(elm));

        // check
        HttpMethod expectedMethod = HttpMethod.Post;
        bool isCorrectHttpMethod = endPoint.HttpMethod == expectedMethod;
        if (!isCorrectHttpMethod)
            throw new InvalidOperationException($"Method {endPoint.Id} at resource path {endPoint.ResourcePath} executes via ${endPoint.HttpMethod} method (${expectedMethod.ToString().ToUpperInvariant()} expected)");

        if (missingParameters.Any())
            throw new ArgumentException($"Missing assignment of {string.Join(',', missingParameters)} in the call of {endPoint.Id} at resource path {endPoint.ResourcePath}");

        bool providesJsonResponses = endPoint.Response?.Representations?
            .Any(repr => repr.MediaType == WadlRepresentation.MediaTypes.Json) ?? false;

        // execute
        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(args),
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(WadlRepresentation.MediaTypeJson));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest);
        }
        catch (Exception ex)
        {
            throw new AddWorkLogException(issueKey, day.ToDateTime(TimeOnly.MinValue), timeSpentSeconds, ex);
        }

        if (response.Content.Headers.ContentType?.MediaType != WadlRepresentation.MediaTypeJson)
            throw new InvalidDataException($"Invalid media type returned ({response.Content.Headers.ContentType?.MediaType ?? string.Empty})");

        await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
    }

    public async Task AddWorkLogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        DateOnly[] daysInPeriod = Enumerable.Range(0, dayFrom.NumberOfDaysTo(dayTo))
            .Select(i => dayFrom.AddDays(i))
            .Where(day => includeNonWorkingDays || day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToArray();

        if (!daysInPeriod.Any())
            return;

        int timeSpentSecondsPerSingleDay = timeSpentSeconds / daysInPeriod.Length;

        Task[] addWorklogTasks = daysInPeriod
            .Select(day => AddWorkLog(issueKey, day, timeSpentSecondsPerSingleDay, activity, comment))
            .ToArray();

        await Task.WhenAll(addWorklogTasks);
    }

    public async Task DeleteWorkLog(long issueId, long worklogId, bool notifyUsers = false)
    {
        await _vanillaJiraApi.DeleteWorkLog(issueId, worklogId, notifyUsers);
    }

    public async Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await _vanillaJiraApi.UpdateWorkLog(issueKey, worklogId, day, timeSpentSeconds, activity, comment);
    }

    private async Task<WadlApplication> GetWADL()
    {
        Uri uri = new Uri($"{_flavourOptions.PluginBaseUri}/application.wadl", UriKind.Relative);
        using Stream response = await _httpClient.GetStreamAsync(uri);

        if (response == null)
            throw new HttpRequestException($"Empty content received from ${uri}");

        XmlSerializer serializer = new XmlSerializer(typeof(WadlApplication));
        object resultObj = serializer.Deserialize(response) ?? throw new InvalidDataException($"Empty/null content deserialization result");

        WadlApplication result = (WadlApplication)resultObj;
        return result;
    }
}
