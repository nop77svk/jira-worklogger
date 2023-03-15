namespace jwl.core;
using NoP77svk.Web.WS;

public static class TempoTimesheetsPluginApiExt
{
    public static async Task AddWorklog(this JiraServerApi self, string issueKey, string userKey, DateTime day, int timeSpentSeconds, TempoWorklogType tempoWorklogType, string comment)
    {
        var request = new api.rest.request.TempoAddWorklogByIssueKey()
        {
            IssueKey = issueKey,
            Worker = userKey,
            Started = day.ToString("yyyy-MM-dd"),
            EndDate = day.ToString("yyyy-MM-dd"),
            TimeSpentSeconds = timeSpentSeconds,
            BillableSeconds = timeSpentSeconds,
            IncludeNonWorkingDays = false,
            Attributes = new Dictionary<string, api.rest.common.TempoWorklogAttribute>()
            {
                [@"_WorklogType_"] = new api.rest.common.TempoWorklogAttribute()
                    {
                        WorkAttributeId = 1,
                        Key = @"_WorklogType_",
                        Name = @"Worklog Type",
                        Type = api.rest.common.TempoWorklogAttributeType.StaticList,
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
}
