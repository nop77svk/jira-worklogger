namespace jwl.jira;
using System.Net.Http.Json;
using jwl.infra;

// https://www.tempo.io/server-api-documentation/timesheets
public static class TempoTimesheetsPluginApiExt
{
    public const string WorklogTypeAttributeKey = @"_WorklogType_";

    public static async Task AddWorklogPeriod(this JiraServerApi self, string issueKey, string userKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string tempoWorklogType, string comment, bool includeNonWorkingDays = false)
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
        await self.HttpClient.PostAsJsonAsync(@"rest/tempo-timesheets/4/worklogs", request);
    }

    public static async Task AddWorklog(this JiraServerApi self, string issueKey, string userKey, DateOnly day, int timeSpentSeconds, string tempoWorklogType, string comment)
    {
        await self.AddWorklogPeriod(issueKey, userKey, day, day, timeSpentSeconds, tempoWorklogType, comment);
    }

    public static async Task DeleteWorklog(this JiraServerApi self, long worklogId)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/tempo-timesheets/4/worklogs")
                .Add(worklogId.ToString())
        };
        await self.HttpClient.DeleteAsync(uriBuilder.Uri);
    }

    public static async Task UpdateWorklogPeriod(this JiraServerApi self, int worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string comment, string tempoWorklogType, bool includeNonWorkingDays = false)
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
        await self.HttpClient.PutAsJsonAsync(uriBuilder.Uri, request);
    }

    public static async Task UpdateWorklog(this JiraServerApi self, int worklogId, DateOnly day, int timeSpentSeconds, string comment, string tempoWorklogType)
    {
        await self.UpdateWorklogPeriod(worklogId, day, day, timeSpentSeconds, comment, tempoWorklogType);
    }

    public static async Task<api.rest.response.TempoWorklogAttributeDefinition[]> GetWorklogAttributesEnum(this JiraServerApi self)
    {
        return await self.HttpClient.GetJsonAsync<api.rest.response.TempoWorklogAttributeDefinition[]>(@"rest/tempo-core/1/work-attribute");
    }

    public static async Task<api.rest.response.TempoWorklog[]> GetIssueWorklogs(this JiraServerApi self, DateOnly from, DateOnly to, IEnumerable<string>? issueKeys, IEnumerable<string>? userKeys)
    {
        var request = new api.rest.request.TempoFindWorklogs(from, to)
        {
            IssueKey = issueKeys?.ToArray(),
            UserKey = userKeys?.ToArray()
        };
        var response = await self.HttpClient.PostAsJsonAsync(@"rest/tempo-timesheets/4/worklogs/search", request);
        return await HttpClientJsonExt.DeserializeJsonStream<api.rest.response.TempoWorklog[]>(await response.Content.ReadAsStreamAsync());
    }
}
