namespace jwl.jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IJiraClient
{
    api.rest.common.JiraUserInfo UserInfo { get; }

    Task<WorkLogType[]> GetAvailableActivities();

    Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, string issueKey);

    Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys);

    Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment);

    Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false);

    Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false);

    Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment);
}
