namespace jwl.jira;
using System.Net.Http.Json;
using jwl.infra;
using jwl.jira.api.rest.common;

// https://www.tempo.io/server-api-documentation/timesheets
public class JiraWithTempoPluginApi
    : IJiraServerApi
{
    private const string WorklogTypeAttributeKey = @"_WorklogType_";

    private readonly HttpClient _httpClient;
    private readonly VanillaJiraServerApi _vanillaJiraServerApi;

    public string UserName { get; }

    public JiraWithTempoPluginApi(HttpClient httpClient, string userName)
    {
        _httpClient = httpClient;
        _vanillaJiraServerApi = new VanillaJiraServerApi(httpClient, userName);

        UserName = userName;
    }

    public async Task<JiraUserInfo> GetUserInfo()
    {
        return await _vanillaJiraServerApi.GetUserInfo();
    }

    public async Task<api.rest.response.TempoWorklogAttributeDefinition[]> GetWorklogAttributeDefinitions()
    {
        return await _httpClient.GetAsJsonAsync<api.rest.response.TempoWorklogAttributeDefinition[]>(@"rest/tempo-core/1/work-attribute");
    }

    public async Task<WorkLogType[]> GetWorklogTypes()
    {
        api.rest.response.TempoWorklogAttributeDefinition[] attrEnumDefs = await GetWorklogAttributeDefinitions();

        var result = attrEnumDefs
            .Where(attrDef => attrDef.Key?.Equals(WorklogTypeAttributeKey) ?? false)
            .Where(attrDef => attrDef.Type != null
                && attrDef.Type?.Value == TempoWorklogAttributeTypeIdentifier.StaticList
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

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        JiraUserInfo userInfo = await GetUserInfo();
        string userKey = userInfo.Key ?? throw new ArgumentNullException($"{nameof(userInfo)}.{nameof(userInfo.Key)}");

        var request = new api.rest.request.TempoFindWorklogs(from, to)
        {
            IssueKey = issueKeys?.ToArray(),
            UserKey = new string[] { userKey }
        };
        var response = await _httpClient.PostAsJsonAsync(@"rest/tempo-timesheets/4/worklogs/search", request);
        var tempoWorkLogs = await HttpClientJsonExt.DeserializeJsonStreamAsync<api.rest.response.TempoWorklog[]>(await response.Content.ReadAsStreamAsync());

        var result = tempoWorkLogs
            .Select(wl => new WorkLog(
                Id: wl.Id ?? -1,
                IssueId: wl.IssueId ?? -1,
                AuthorName: wl.WorkerKey == userKey ? UserName : null,
                AuthorKey: wl.WorkerKey,
                Created: wl.Created?.Value ?? DateTime.MinValue,
                Started: wl.Started?.Value ?? DateTime.MinValue,
                TimeSpentSeconds: wl.TimeSpentSeconds ?? -1,
                WorkLogType: wl.Attributes?[WorklogTypeAttributeKey].Value,
                Comment: wl.Comment ?? string.Empty
            ))
            .ToArray();

        return result;
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await AddWorklogPeriod(issueKey, day, day, timeSpentSeconds, activity, comment);
    }

    public async Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? tempoWorklogType, string? comment, bool includeNonWorkingDays = false)
    {
        JiraUserInfo userInfo = await GetUserInfo();
        string userKey = userInfo.Key ?? throw new ArgumentNullException($"{nameof(userInfo)}.{nameof(userInfo.Key)}");

        var request = new api.rest.request.TempoAddWorklogByIssueKey()
        {
            IssueKey = issueKey,
            Worker = userKey,
            Started = new api.rest.common.TempoDate(dayFrom),
            EndDate = new api.rest.common.TempoDate(dayTo),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = includeNonWorkingDays,
            Comment = comment,
            Attributes = new Dictionary<string, api.rest.common.TempoWorklogAttribute>()
            {
                [WorklogTypeAttributeKey] = new api.rest.common.TempoWorklogAttribute()
                    {
                        WorkAttributeId = 1,
                        Key = WorklogTypeAttributeKey,
                        Name = @"Worklog Type",
                        Type = api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList,
                        Value = tempoWorklogType
                    }
            }
        };

        await _httpClient.PostAsJsonAsync(@"rest/tempo-timesheets/4/worklogs", request);
    }

    public async Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/tempo-timesheets/4/worklogs")
                .Add(worklogId.ToString())
        };
        await _httpClient.DeleteAsync(uriBuilder.Uri.PathAndQuery);
    }

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await UpdateWorklogPeriod(issueKey, worklogId, day, day, timeSpentSeconds, comment, activity);
    }

    private async Task UpdateWorklogPeriod(string issueKey, long worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? comment, string? activity, bool includeNonWorkingDays = false)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/tempo-timesheets/4/worklogs")
                .Add(worklogId.ToString())
        };
        var request = new api.rest.request.TempoUpdateWorklog()
        {
            Started = new api.rest.common.TempoDate(dayFrom),
            EndDate = new api.rest.common.TempoDate(dayTo),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = includeNonWorkingDays,
            Comment = comment,
            Attributes = new Dictionary<string, api.rest.common.TempoWorklogAttribute>()
            {
                [WorklogTypeAttributeKey] = new api.rest.common.TempoWorklogAttribute()
                {
                    WorkAttributeId = 1,
                    Key = WorklogTypeAttributeKey,
                    Name = @"Worklog Type",
                    Type = api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList,
                    Value = activity
                }
            }
        };
        await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }
}
