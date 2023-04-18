namespace jwl.jira;
using NoP77svk.Web.WS;

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

        string result = await self.WsClient.EndpointGetString(new JsonRestWsEndpoint(HttpMethod.Post)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"tempo-timesheets")
            .AddResourceFolder(@"4")
            .AddResourceFolder(@"worklogs")
            .WithContent(request)
        );
    }

    public static async Task AddWorklog(this JiraServerApi self, string issueKey, string userKey, DateOnly day, int timeSpentSeconds, string tempoWorklogType, string comment)
    {
        await self.AddWorklogPeriod(issueKey, userKey, day, day, timeSpentSeconds, tempoWorklogType, comment);
    }

    public static async Task UpdateWorklogPeriod(this JiraServerApi self, int worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string comment, string tempoWorklogType, bool includeNonWorkingDays = false)
    {
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

        await self.WsClient.EndpointCall(new JsonRestWsEndpoint(HttpMethod.Put)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"tempo-timesheets")
            .AddResourceFolder(@"4")
            .AddResourceFolder(@"worklogs")
            .AddResourceFolder(worklogId.ToString())
            .WithContent(request)
        );
    }

    public static async Task UpdateWorklog(this JiraServerApi self, int worklogId, DateOnly day, int timeSpentSeconds, string comment, string tempoWorklogType)
    {
        await self.UpdateWorklogPeriod(worklogId, day, day, timeSpentSeconds, comment, tempoWorklogType);
    }

    public static async Task<api.rest.response.TempoWorklogAttributeDefinition[]> GetWorklogAttributesEnum(this JiraServerApi self)
    {
        IAsyncEnumerable<api.rest.response.TempoWorklogAttributeDefinition[]> response = self.WsClient.EndpointGetObject<api.rest.response.TempoWorklogAttributeDefinition[]>(new JsonRestWsEndpoint(HttpMethod.Get)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"tempo-core")
            .AddResourceFolder(@"1")
            .AddResourceFolder(@"work-attribute")
        );

        return await response.FirstAsync();
    }

    public static async Task<api.rest.response.TempoWorklog[]> GetIssueWorklogs(this JiraServerApi self, DateOnly from, DateOnly to, IEnumerable<string>? issueKeys, IEnumerable<string>? userKeys)
    {
        var request = new api.rest.request.TempoFindWorklogs(from, to)
        {
            IssueKey = issueKeys?.ToArray(),
            UserKey = userKeys?.ToArray()
        };

        IAsyncEnumerable<api.rest.response.TempoWorklog[]> response = self.WsClient.EndpointGetObject<api.rest.response.TempoWorklog[]>(new JsonRestWsEndpoint(HttpMethod.Post)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"tempo-timesheets")
            .AddResourceFolder(@"4")
            .AddResourceFolder(@"worklogs")
            .AddResourceFolder(@"search")
            .WithContent(request)
        );

        return await response.FirstAsync();
    }
}
