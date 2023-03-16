namespace jwl.core;
using NoP77svk.Web.WS;

public static class TempoTimesheetsPluginApiExt
{
    public static async Task AddWorklogPeriod(this JiraServerApi self, string issueKey, string userKey, DateTime dayFrom, DateTime dayTo, int timeSpentSeconds, TempoWorklogType tempoWorklogType, string comment, bool includeNonWorkingDays = false)
    {
        var request = new api.rest.request.TempoAddWorklogByIssueKey()
        {
            IssueKey = issueKey,
            Worker = userKey,
            Started = dayFrom.ToString("yyyy-MM-dd"),
            EndDate = dayTo.ToString("yyyy-MM-dd"),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = includeNonWorkingDays,
            Attributes = new Dictionary<string, api.rest.common.TempoWorklogAttribute>()
            {
                [@"_WorklogType_"] = new api.rest.common.TempoWorklogAttribute()
                    {
                        WorkAttributeId = 1,
                        Key = @"_WorklogType_",
                        Name = @"Worklog Type",
                        Type = api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList,
                        Value = tempoWorklogType
                    }
            }
        };

        await self.WsClient.EndpointCall(new JsonRestWsEndpoint(HttpMethod.Post)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"tempo-timesheets")
            .AddResourceFolder(@"4")
            .AddResourceFolder(@"worklogs")
            .WithContent(request)
        );
    }

    public static async Task AddWorklog(this JiraServerApi self, string issueKey, string userKey, DateTime day, int timeSpentSeconds, TempoWorklogType tempoWorklogType, string comment)
    {
        await self.AddWorklogPeriod(issueKey, userKey, day, day, timeSpentSeconds, tempoWorklogType, comment);
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
}
