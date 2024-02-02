namespace jwl.jira;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using jwl.infra;
using jwl.wadl;

// https://interconcept.atlassian.net/wiki/spaces/ICTIME/pages/31686672/API
// https://interconcept.atlassian.net/wiki/spaces/ICBIZ/pages/34701333/REST+Services
public class JiraWithICTimePluginApi
    : IJiraClient
{
    public string UserName { get; }
    public api.rest.common.JiraUserInfo UserInfo => _vanillaJiraApi.UserInfo;

    public string PluginBaseUri { get; } = "rest/ictime/1.0";
    public Lazy<Dictionary<string, wadl.ComposedWadlMethodDefinition>> Endpoints =>
        new Lazy<Dictionary<string, ComposedWadlMethodDefinition>>(() => this.GetWADL().Result
            .AsEnumerable()
            .Where(res => !string.IsNullOrEmpty(res.Id))
            .ToDictionary(res => res.Id ?? string.Empty)
        );

    public const string CreateWorkLogMethodName = "createWorklog";
    public wadl.ComposedWadlMethodDefinition CreateWorkLogMethodDefinition => Endpoints.Value[CreateWorkLogMethodName];

    public const string GetActivityTypesForProjectMethodName = "getActivityTypesForProject";
    public wadl.ComposedWadlMethodDefinition GetActivityTypesForProjectMethodDefinition => Endpoints.Value[GetActivityTypesForProjectMethodName];

    private readonly HttpClient _httpClient;
    private readonly VanillaJiraClient _vanillaJiraApi;

    public JiraWithICTimePluginApi(HttpClient httpClient, string userName)
    {
        _httpClient = httpClient;
        UserName = userName;
        _vanillaJiraApi = new VanillaJiraClient(httpClient, userName);
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
            .Where(par => !string.IsNullOrEmpty(par.Name))
            .Select(par => par.Name ?? string.Empty)
            .ToHashSet();

        // define
        IEnumerable<KeyValuePair<JiraIssueKey, string>> uris = distinctProjects
            .Select(issueKey => new KeyValuePair<JiraIssueKey, string>(issueKey, endPoint.ResourcePath.Replace("{issueKey}", issueKey.ToString())));

        missingParameters.Remove("issueKey");

        // check
        if (missingParameters.Any())
            throw new ArgumentNullException($"Missing assignment of {string.Join(',', missingParameters)} in the call of {endPoint.Id} at resource path {endPoint.ResourcePath}");

        bool providesJsonResponses = endPoint.Response?.Representations?
            .Any(repr => repr.MediaType == WadlRepresentation.MediaTypes.Json) ?? false;

        if (providesJsonResponses)
            throw new InvalidDataException($"Method {endPoint.Id} at resource path {endPoint.ResourcePath} does not respond in JSON");

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

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, string issueKey)
    {
        return await _vanillaJiraApi.GetIssueWorklogs(from, to, issueKey);
    }

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        return await _vanillaJiraApi.GetIssueWorklogs(from, to, issueKeys);
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };

        // 2do! annotate the request.CustomFieldNNN with JSON field name based on ICTime server metadata (retrieved previously)
        var request = new api.rest.request.ICTimeAddWorklogByIssueKey(
            IssueKey: issueKey,
            Started: day
                .ToDateTime(TimeOnly.MinValue)
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds: timeSpentSeconds,
            Activity: string.IsNullOrEmpty(activity) ? null : int.Parse(activity),
            Comment: comment
        );

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
        await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
    }

    public async Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        DateOnly[] daysInPeriod = Enumerable.Range(0, dayFrom.NumberOfDaysTo(dayTo))
            .Select(i => dayFrom.AddDays(i))
            .Where(day => includeNonWorkingDays || day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToArray();

        if (!daysInPeriod.Any())
            return;

        int timeSpentSecondsPerSingleDay = timeSpentSeconds / daysInPeriod.Length;

        Task[] addWorklogTasks = daysInPeriod
            .Select(day => AddWorklog(issueKey, day, timeSpentSecondsPerSingleDay, activity, comment))
            .ToArray();

        await Task.WhenAll(addWorklogTasks);
    }

    public async Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false)
    {
        await _vanillaJiraApi.DeleteWorklog(issueId, worklogId, notifyUsers);
    }

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await _vanillaJiraApi.UpdateWorklog(issueKey, worklogId, day, timeSpentSeconds, activity, comment);
    }

    private async Task<WadlApplication> GetWADL()
    {
        Uri uri = new Uri($"{PluginBaseUri}/application.wadl", UriKind.Relative);
        using Stream response = await _httpClient.GetStreamAsync(uri);
        if (response == null || response.Length <= 0)
            throw new HttpRequestException($"Empty content received from ${uri}");

        XmlSerializer serializer = new XmlSerializer(typeof(WadlApplication));
        object resultObj = serializer.Deserialize(response) ?? throw new InvalidDataException($"Empty/null content deserialization result");

        WadlApplication result = (WadlApplication)resultObj;
        return result;
    }
}
