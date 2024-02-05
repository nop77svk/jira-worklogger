namespace jwl.jira;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using jwl.infra;
using jwl.jira.api.rest.response;

public class VanillaJiraClient
    : IJiraClient
{
    public string UserName { get; }
    public api.rest.common.JiraUserInfo UserInfo => _lazyUserInfo.Value;

    private readonly HttpClient _httpClient;
    private readonly Lazy<jwl.jira.api.rest.common.JiraUserInfo> _lazyUserInfo;

    public VanillaJiraClient(HttpClient httpClient, string userName)
    {
        _httpClient = httpClient;
        UserName = userName;
        _lazyUserInfo = new Lazy<api.rest.common.JiraUserInfo>(() => GetUserInfo().Result);
    }

    public static async Task CheckHttpResponseForErrorMessages(HttpResponseMessage responseMessage)
    {
        using Stream responseContentStream = await responseMessage.Content.ReadAsStreamAsync();

        if (responseContentStream.Length > 0)
        {
            JiraRestResponse responseContent = await HttpClientJsonExt.DeserializeJsonStreamAsync<JiraRestResponse>(responseContentStream);
            if (responseContent?.ErrorMessages is not null && responseContent.ErrorMessages.Any())
                throw new InvalidOperationException(string.Join(Environment.NewLine, responseContent.ErrorMessages));
        }
    }

    #pragma warning disable CS1998
    public async Task<WorkLogType[]> GetAvailableActivities(string issueKey)
    {
        return Array.Empty<WorkLogType>();
    }
    #pragma warning restore

    public async Task<Dictionary<string, WorkLogType[]>> GetAvailableActivities(IEnumerable<string> issueKeys)
    {
        WorkLogType[] activities = await GetAvailableActivities(string.Empty);

        Dictionary<string, WorkLogType[]> result = issueKeys
            .Select(issueKey => new ValueTuple<string, WorkLogType[]>(issueKey, activities))
            .ToDictionary(
                keySelector: x => x.Item1,
                elementSelector: x => x.Item2
            );

        return result;
    }

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, string issueKey)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };

        var response = await _httpClient.GetAsJsonAsync<api.rest.response.JiraIssueWorklogs>(uriBuilder.Uri.PathAndQuery);

        (DateTime minDt, DateTime supDt) = DateOnlyUtils.DateOnlyRangeToDateTimeRange(from, to);

        var result = response.Worklogs
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
                Activity: null,
                Comment: wl.Comment
            ))
            .ToArray();

        return result;
    }

    public async Task<WorkLog[]> GetIssueWorkLogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        if (issueKeys is null)
            return Array.Empty<WorkLog>();

        Task<WorkLog[]>[] responseTasks = issueKeys
            .Distinct()
            .Select(issueKey => GetIssueWorkLogs(from, to, issueKey))
            .ToArray();

        await Task.WhenAll(responseTasks);

        var result = responseTasks
            .SelectMany(task => task.Result)
            .ToArray();

        return result;
    }

    public async Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };

        StringBuilder commentBuilder = new StringBuilder();
        if (activity != null)
            commentBuilder.Append($"({activity}){Environment.NewLine}");
        commentBuilder.Append(comment);

        var request = new api.rest.request.JiraAddWorklogByIssueKey(
            Started: day
                .ToDateTime(TimeOnly.MinValue)
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds: timeSpentSeconds,
            Comment: commentBuilder.ToString()
        );

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
        await CheckHttpResponseForErrorMessages(response);
    }

    public async Task AddWorkLogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        DateOnly[] daysInPeriod = Enumerable.Range(0, dayFrom.NumberOfDaysTo(dayTo))
            .Select(i => dayFrom.AddDays(i))
            .Where(day => includeNonWorkingDays || day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToArray();

        if (!daysInPeriod.Any())
            return;

        int timeSpentSecondsPerSingleDay = timeSpentSeconds / daysInPeriod.Length;

        Task[] addWorklogTasks = daysInPeriod
            .Select(day => AddWorkLog(issueKey, day, timeSpentSecondsPerSingleDay, activity, comment))
            .ToArray();

        await Task.WhenAll(addWorklogTasks);
    }

    public async Task DeleteWorkLog(long issueId, long worklogId, bool notifyUsers = false)
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

        HttpResponseMessage response = await _httpClient.DeleteAsync(uriBuilder.Uri.PathAndQuery);
        await CheckHttpResponseForErrorMessages(response);
    }

    public async Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
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

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
        await CheckHttpResponseForErrorMessages(response);
    }

    private async Task<api.rest.common.JiraUserInfo> GetUserInfo()
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = @"rest/api/2/user",
            Query = new UriQueryBuilder()
                .Add(@"username", UserName)
        };
        return await _httpClient.GetAsJsonAsync<api.rest.common.JiraUserInfo>(uriBuilder.Uri.PathAndQuery);
    }
}
