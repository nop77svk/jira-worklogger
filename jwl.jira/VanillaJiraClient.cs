namespace jwl.jira;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Xml.Linq;
using jwl.infra;

public class VanillaJiraClient
    : IJiraClient
{
    public string UserName { get; }

    private readonly HttpClient _httpClient;

    public VanillaJiraClient(HttpClient httpClient, string userName)
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

    #pragma warning disable CS1998
    public async Task<WorkLogType[]> GetWorklogTypes()
    {
        return Array.Empty<WorkLogType>();
    }
    #pragma warning restore

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        if (issueKeys is null)
            return Array.Empty<WorkLog>();

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

        (DateTime minDt, DateTime supDt) = DateOnlyUtils.DateOnlyRangeToDateTimeRange(from, to);

        var result = responseTasks
            .SelectMany(task => task.Result.Worklogs)
            .Where(worklog => worklog.Author.Name == UserName)
            .Where(worklog => worklog.Started.Value >= minDt && worklog.Started.Value < supDt)
            .Select(wl => new WorkLog(
                Id: wl.Id.Value,
                IssueId: wl.IssueId.Value,
                AuthorName: wl.Author.Name,
                AuthorKey: wl.Author.Key,
                Created: wl.Created.Value,
                Started: wl.Started.Value,
                TimeSpentSeconds: wl.TimeSpentSeconds,
                WorkLogType: null,
                Comment: wl.Comment
            ))
            .ToArray();

        return result;
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };
        var request = new api.rest.request.JiraAddWorklogByIssueKey(
            Started: day
                .ToDateTime(TimeOnly.MinValue)
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds: timeSpentSeconds,
            Comment: comment
        );
        await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }

    public async Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        DateOnly[] daysInPeriod = Enumerable.Range(0, dayFrom.NumberOfDaysTo(dayTo))
            .Select(i => dayFrom.AddDays(i))
            .Where(day => includeNonWorkingDays || day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToArray();

        if (!daysInPeriod.Any())
            return;

        int timeSpentSecondsPerSingleDay = timeSpentSeconds / daysInPeriod.Length;

        Task[] addWorklogTasks = daysInPeriod
            .Select(day => AddWorklog(issueKey, day, timeSpentSecondsPerSingleDay, activity, comment))
            .ToArray();

        await Task.WhenAll(addWorklogTasks);
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

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
                .Add(worklogId.ToString())
        };
        var request = new api.rest.request.JiraAddWorklogByIssueKey(
            Started: day
                .ToDateTime(TimeOnly.MinValue)
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds: timeSpentSeconds,
            Comment: comment
        );
        await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
    }
}
