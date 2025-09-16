namespace jwl.jira.Flavours;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using jwl.jira.api.rest.common;

public class VanillaCloudV2JiraClient
    : IJiraClient
{
    private readonly HttpClient _httpClient;
    private readonly string _userName;
    private readonly FlavourCloudV2Options? _flavourOptions;

    public JiraUserInfo UserInfo => throw new NotImplementedException();

    public VanillaCloudV2JiraClient(HttpClient httpClient, string userName, FlavourCloudV2Options? flavourOptions)
    {
        _httpClient = httpClient;
        _userName = userName;
        _flavourOptions = flavourOptions;
    }

    public Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment) => throw new NotImplementedException();

    public Task AddWorkLogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false) => throw new NotImplementedException();

    public Task DeleteWorkLog(long issueId, long worklogId, bool notifyUsers = false) => throw new NotImplementedException();

    public Task<WorkLogType[]> GetAvailableActivities(string issueKey) => throw new NotImplementedException();

    public Task<Dictionary<string, WorkLogType[]>> GetAvailableActivities(IEnumerable<string> issueKeys) => throw new NotImplementedException();

    public Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, string issueKey) => throw new NotImplementedException();

    public Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys) => throw new NotImplementedException();

    public Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment) => throw new NotImplementedException();
}