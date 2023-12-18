namespace jwl.jira;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using jwl.infra;

public class JiraServerApi
{
    public HttpClient HttpClient { get; }

    public JiraServerApi(HttpClient httpClient)
    {
        this.HttpClient = httpClient;
    }

    public async Task<api.rest.common.JiraUserInfo> GetUserInfo(string userName)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = @"rest/api/2/user",
            Query = new UriQueryBuilder()
                .Add(@"username", userName)
        };
        return await HttpClient.GetJsonAsync<api.rest.common.JiraUserInfo>(uriBuilder.Uri);
    }

    public async Task<api.rest.response.JiraIssueWorklogs> GetIssueWorklogs(string issueKey)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };
        return await HttpClient.GetJsonAsync<api.rest.response.JiraIssueWorklogs>(uriBuilder.Uri);
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
        await HttpClient.DeleteAsync(uriBuilder.Uri);
    }

    public async Task AddWorklog(string issueKey, DateTime day, int timeSpentSeconds, string comment)
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
        await HttpClient.PostAsJsonAsync(uriBuilder.Uri, request);
    }

    public async Task UpdateWorklog(string issueKey, int worklogId, DateTime day, int timeSpentSeconds, string comment)
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
        await HttpClient.PutAsJsonAsync(uriBuilder.Uri, request);
    }
}
