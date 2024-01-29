﻿namespace jwl.jira;
using jwl.jira.api.rest.common;

// https://interconcept.atlassian.net/wiki/spaces/ICTIME/pages/31686672/API
public class JiraWithICTimePluginApi
    : IJiraServerApi
{
    public HttpClient HttpClient { get; }
    public string UserName { get; }

    public JiraWithICTimePluginApi(HttpClient httpClient, string userName)
    {
        HttpClient = httpClient;
        UserName = userName;
        _vanillaJiraApi = new VanillaJiraServerApi(httpClient, userName);
    }

    private readonly VanillaJiraServerApi _vanillaJiraApi;

    public async Task<JiraUserInfo> GetUserInfo()
    {
        return await _vanillaJiraApi.GetUserInfo();
    }

    public Task<WorkLogType[]> GetWorklogTypes()
    {
        throw new NotImplementedException();
    }

    public Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        throw new NotImplementedException();
    }

    public Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        throw new NotImplementedException();
    }

    public Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? tempoWorklogType, string? comment, bool includeNonWorkingDays = false)
    {
        throw new NotImplementedException();
    }

    public Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false)
    {
        throw new NotImplementedException();
    }

    public Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        throw new NotImplementedException();
    }
}