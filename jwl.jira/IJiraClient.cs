namespace jwl.Jira;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IJiraClient
{
    Contract.Rest.Common.JiraUserInfo UserInfo { get; }

    Task<WorkLogType[]> GetAvailableActivities(string issueKey);

    Task<Dictionary<string, WorkLogType[]>> GetAvailableActivities(IEnumerable<string> issueKeys);

    Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, string issueKey);

    Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys);

    Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment);

    Task AddWorkLogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false);

    Task DeleteWorkLog(long issueId, long worklogId, bool notifyUsers = false);

    Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment);
}