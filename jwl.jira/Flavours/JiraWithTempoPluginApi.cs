namespace jwl.Jira;

using System.Net.Http.Json;

using jwl.Infra;
using jwl.Jira.Contract.Rest.Common;
using jwl.Jira.Exceptions;
using jwl.Jira.Flavours;

// https://www.tempo.io/server-api-documentation/timesheets
public class JiraWithTempoPluginApi
    : IJiraClient
{
    private const string WorklogTypeAttributeKey = @"_WorklogType_";

    private readonly HttpClient _httpClient;
    private readonly FlavourTempoTimesheetsOptions _flavourOptions;
    private readonly VanillaJiraClient _vanillaJiraApi;

    public string UserName { get; }
    public Contract.Rest.Common.JiraUserInfo CurrentUser => _vanillaJiraApi.CurrentUser;

    public JiraWithTempoPluginApi(HttpClient httpClient, string userName, VanillaJiraClient vanillaJiraClient, FlavourTempoTimesheetsOptions? flavourOptions)
    {
        _httpClient = httpClient;
        UserName = userName;
        _flavourOptions = flavourOptions ?? new FlavourTempoTimesheetsOptions();
        _vanillaJiraApi = vanillaJiraClient;
    }

    public async Task<Contract.Rest.Response.TempoWorklogAttributeDefinition[]> GetWorklogAttributeDefinitions()
    {
        return await _httpClient.GetAsJsonAsync<Contract.Rest.Response.TempoWorklogAttributeDefinition[]>($"{_flavourOptions.PluginCoreUri}/work-attribute");
    }

    public async Task<WorkLogType[]> GetAvailableActivities(string issueKey)
    {
        Contract.Rest.Response.TempoWorklogAttributeDefinition[] attrEnumDefs = await GetWorklogAttributeDefinitions();

        WorkLogType[] result = attrEnumDefs
            .Where(attrDef => attrDef.Key?.Equals(WorklogTypeAttributeKey) ?? false)
            .Where(attrDef => attrDef.Type != null
                && attrDef.Type?.Value == Contract.Rest.Common.TempoWorklogAttributeTypeIdentifier.StaticList
            )
            .SelectMany(attrDef => attrDef.StaticListValues)
            .Where(staticListItem => !string.IsNullOrEmpty(staticListItem.Name) && !string.IsNullOrEmpty(staticListItem.Value))
            .Select(staticListItem => new WorkLogType(
                Key: staticListItem.Name ?? string.Empty,
                Value: staticListItem.Value ?? string.Empty,
                Sequence: staticListItem.Sequence ?? -1
            ))
            .ToArray();

        return result;
    }

    public async Task<Dictionary<string, WorkLogType[]>> GetAvailableActivities(IEnumerable<string> issueKeys)
    {
        WorkLogType[] activities = await GetAvailableActivities(string.Empty);

        Dictionary<string, WorkLogType[]> result = issueKeys
            .Select(issueKey => new ValueTuple<string, WorkLogType[]>(issueKey, activities))
            .ToDictionary(
                keySelector: x => x.Item1,
                elementSelector: x => x.Item2
            );

        return result;
    }

#pragma warning disable SA1010

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, string issueKey)
    {
        return await GetIssueWorkLogs(from, to, [issueKey]);
    }

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        string userKey = CurrentUser.Key
            ?? throw new JiraClientException($"{nameof(CurrentUser)}.{nameof(CurrentUser.Key)} is NULL");

        var request = new Contract.Rest.Request.TempoFindWorklogs(from, to)
        {
            IssueKey = issueKeys?.ToArray(),
            UserKey = [userKey]
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_flavourOptions.PluginBaseUri}/worklogs/search", request);
            var tempoWorkLogs = await HttpClientExt.DeserializeJsonStreamAsync<Contract.Rest.Response.TempoWorklog[]>(await response.Content.ReadAsStreamAsync());

            var result = tempoWorkLogs
                .Select(wl => new WorkLog(
                    Id: wl.Id ?? -1,
                    IssueId: wl.IssueId ?? -1,
                    AuthorName: wl.WorkerKey == userKey ? UserName : null,
                    AuthorKey: wl.WorkerKey,
                    Created: wl.Created?.Value ?? DateTime.MinValue,
                    Started: wl.Started?.Value ?? DateTime.MinValue,
                    TimeSpentSeconds: wl.TimeSpentSeconds ?? -1,
                    Activity: wl.Attributes?[WorklogTypeAttributeKey].Value,
                    Comment: wl.Comment ?? string.Empty
                ))
                .ToArray();

            return result;
        }
        catch (Exception ex)
        {
            throw new JiraGetIssueWorkLogsException("[multiple]", from.ToDateTime(TimeOnly.MinValue), to.ToDateTime(TimeOnly.MinValue), ex);
        }
    }

#pragma warning restore SA1010

    public async Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await AddWorkLogPeriod(issueKey, day, day, timeSpentSeconds, activity, comment);
    }

    public async Task AddWorkLogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        string userKey = CurrentUser.Key
            ?? throw new JiraClientException($"NULL {nameof(CurrentUser)}.{nameof(CurrentUser.Key)}");

        var request = new Contract.Rest.Request.TempoAddWorklogByIssueKey()
        {
            IssueKey = issueKey,
            Worker = userKey,
            Started = new Contract.Rest.Common.TempoDate(dayFrom),
            EndDate = new Contract.Rest.Common.TempoDate(dayTo),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = includeNonWorkingDays,
            Comment = comment,
            Attributes = new Dictionary<string, Contract.Rest.Common.TempoWorklogAttribute>()
            {
                [WorklogTypeAttributeKey] = new Contract.Rest.Common.TempoWorklogAttribute()
                {
                    WorkAttributeId = 1,
                    Key = WorklogTypeAttributeKey,
                    Name = @"Worklog Type",
                    Type = Contract.Rest.Common.TempoWorklogAttributeTypeIdentifier.StaticList,
                    Value = activity
                }
            }
        };

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{_flavourOptions.PluginBaseUri}/worklogs", request);
            await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
        }
        catch (Exception ex)
        {
            throw new JiraAddWorklogsPeriodException(issueKey, dayFrom.ToDateTime(TimeOnly.MinValue), dayTo.ToDateTime(TimeOnly.MinValue), timeSpentSeconds, ex);
        }
    }

    public async Task DeleteWorkLog(long issueId, long worklogId, bool notifyUsers = false)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/worklogs")
                .Add(worklogId.ToString())
        };

        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(uriBuilder.Uri.PathAndQuery);
            await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
        }
        catch (Exception ex)
        {
            throw new JiraDeleteWorklogByIssueIdException(issueId, worklogId, ex);
        }
    }

    public async Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await UpdateWorklogPeriod(worklogId, day, day, timeSpentSeconds, comment, activity);
    }

    private async Task UpdateWorklogPeriod(long worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? comment, string? activity, bool includeNonWorkingDays = false)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/worklogs")
                .Add(worklogId.ToString())
        };
        var request = new Contract.Rest.Request.TempoUpdateWorklog()
        {
            Started = new Contract.Rest.Common.TempoDate(dayFrom),
            EndDate = new Contract.Rest.Common.TempoDate(dayTo),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = includeNonWorkingDays,
            Comment = comment,
            Attributes = new Dictionary<string, Contract.Rest.Common.TempoWorklogAttribute>()
            {
                [WorklogTypeAttributeKey] = new Contract.Rest.Common.TempoWorklogAttribute()
                {
                    WorkAttributeId = 1,
                    Key = WorklogTypeAttributeKey,
                    Name = @"Worklog Type",
                    Type = Contract.Rest.Common.TempoWorklogAttributeTypeIdentifier.StaticList,
                    Value = activity
                }
            }
        };

        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
            await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
        }
        catch (Exception ex)
        {
            throw new JiraUpdateWorklogsPeriodException(issueKey, worklogId, dayFrom.ToDateTime(TimeOnly.MinValue), dayTo.ToDateTime(TimeOnly.MaxValue), timeSpentSeconds, ex);
        }
    }
}