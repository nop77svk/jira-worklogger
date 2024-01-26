namespace jwl.jira;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.AccessControl;
using jwl.infra;
using jwl.jira.api.rest.response;

public class VanillaJiraServerApi
    : IJiraServerApi
{
    private readonly HttpClient _httpClient;

    public string UserName { get; }

    public VanillaJiraServerApi(HttpClient httpClient, string userName)
    {
        _httpClient = httpClient;
        UserName = userName;
    }

    public async Task<api.rest.common.JiraUserInfo> GetUserInfo()
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = @"rest/api/2/user",
            Query = new UriQueryBuilder()
                .Add(@"username", UserName)
        };
        return await _httpClient.GetAsJsonAsync<api.rest.common.JiraUserInfo>(uriBuilder.Uri.PathAndQuery);
    }

    public async Task<TempoWorklogAttributeDefinition[]> GetWorklogTypes()
    {
        return Array.Empty<TempoWorklogAttributeDefinition>();
    }

    public async Task<api.rest.response.JiraIssueWorklogsWorklog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        if (issueKeys is null)
            return Array.Empty<api.rest.response.JiraIssueWorklogsWorklog>();

        Task<api.rest.response.JiraIssueWorklogs>[] responseTasks = issueKeys
            .Distinct()
            .Select(issueKey => new UriBuilder()
            {
                Path = new UriPathBuilder(@"rest/api/2/issue")
                    .Add(issueKey)
                    .Add(@"worklog")
            })
            .Select(uriBuilder => _httpClient.GetAsJsonAsync<api.rest.response.JiraIssueWorklogs>(uriBuilder.Uri.PathAndQuery))
            .ToArray();

        await Task.WhenAll(responseTasks);

        Date

        return responseTasks
            .SelectMany(task => task.Result.Worklogs)
            .Where(worklog => worklog.Started.Value >= from.ToDateTime(TimeOnly.MinValue) && worklog.Started.Value <
            .ToArray();
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };
        var request = new api.rest.request.JiraAddWorklogByIssueKey()
        {
            Started = day
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds = timeSpentSeconds,
            Comment = comment
        };
        await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }

    public Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? worklogType, string? comment, bool includeNonWorkingDays = false)
    {
        throw new NotImplementedException(); // 2do!
    }

    public async Task DeleteWorklog(long issueId, long worklogId, bool notifyUsers = false)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueId.ToString())
                .Add(@"worklog")
                .Add(worklogId.ToString()),
            Query = new UriQueryBuilder()
                .Add(@"notifyUsers", notifyUsers.ToString().ToLower())
        };
        await _httpClient.DeleteAsync(uriBuilder.Uri.PathAndQuery);
    }

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? worklogType, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
                .Add(worklogId.ToString())
        };
        var request = new api.rest.request.JiraAddWorklogByIssueKey()
        {
            Started = day
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds = timeSpentSeconds,
            Comment = comment
        };
        await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }

    public Task UpdateWorklogPeriod(string issueKey, long worklogId, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? comment, string? tempoWorklogType, bool includeNonWorkingDays = false)
    {
        throw new NotImplementedException();
    }
}
