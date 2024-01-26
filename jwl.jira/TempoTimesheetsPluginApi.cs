namespace jwl.jira;
using System.Net.Http.Json;
using jwl.infra;
using jwl.jira.api.rest.common;
using jwl.jira.api.rest.response;

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
        return await _vanillaJiraServerApi.GetUserInfo(UserName);
    }

    public async Task<api.rest.response.TempoWorklogAttributeDefinition[]> GetWorklogTypes()
    {
        return await _httpClient.GetAsJsonAsync<api.rest.response.TempoWorklogAttributeDefinition[]>(@"rest/tempo-core/1/work-attribute");
    }

    public async Task<api.rest.response.TempoWorklog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        JiraUserInfo userInfo = await GetUserInfo();

        var request = new api.rest.request.TempoFindWorklogs(from, to)
        {
            IssueKey = issueKeys?.ToArray(),
            UserKey = new string[] { userInfo.Key }
        };
        var response = await _httpClient.PostAsJsonAsync(@"rest/tempo-timesheets/4/worklogs/search", request);
        return await HttpClientJsonExt.DeserializeJsonStreamAsync<api.rest.response.TempoWorklog[]>(await response.Content.ReadAsStreamAsync());
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        await AddWorklogPeriod(issueKey, day, day, timeSpentSeconds, worklogType, comment);
    }

    public async Task AddWorklogPeriod(string issueKey, string userKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? tempoWorklogType, string? comment, bool includeNonWorkingDays = false)
    {
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

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        await UpdateWorklogPeriod(issueKey, worklogId, day, day, timeSpentSeconds, comment, worklogType);
    }

    public async Task UpdateWorklogPeriod(string issueKey, long worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? comment, string? tempoWorklogType, bool includeNonWorkingDays = false)
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
                    Value = tempoWorklogType
                }
            }
        };
        await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }
}
