namespace jwl.jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal interface IJiraServerApi
{
    Task<api.rest.common.JiraUserInfo> GetUserInfo();

    Task<api.rest.response.TempoWorklogAttributeDefinition[]> GetWorklogTypes();

    Task<api.rest.response.TempoWorklog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys, IEnumerable<string>? userKeys);
    
    Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment);

    Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? tempoWorklogType, string? comment, bool includeNonWorkingDays = false);

    Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false);

    Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment);

    Task UpdateWorklogPeriod(string issueKey, long worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? comment, string? tempoWorklogType, bool includeNonWorkingDays = false);
}
