namespace jwl.jira;

using jwl.infra;
using jwl.jira.api.rest.common;
using jwl.jira.api.rest.response;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Numerics;
using System.Text;

// https://interconcept.atlassian.net/wiki/spaces/ICTIME/pages/31686672/API
// https://interconcept.atlassian.net/wiki/spaces/ICBIZ/pages/34701333/REST+Services
public class JiraWithICTimePluginApi
    : IJiraClient
{
    public string UserName { get; }

    private readonly HttpClient _httpClient;

    public JiraWithICTimePluginApi(HttpClient httpClient, string userName)
    {
        _httpClient = httpClient;
        UserName = userName;
        _vanillaJiraApi = new VanillaJiraClient(httpClient, userName);
    }

    private readonly VanillaJiraClient _vanillaJiraApi;

    public async Task<JiraUserInfo> GetUserInfo()
    {
        return await _vanillaJiraApi.GetUserInfo();
    }

    public async Task<WorkLogType[]> GetAvailableActivities()
    {
        return await _vanillaJiraApi.GetAvailableActivities();
    }

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, string issueKey)
    {
        return await _vanillaJiraApi.GetIssueWorklogs(from, to, issueKey);
    }

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        return await _vanillaJiraApi.GetIssueWorklogs(from, to, issueKeys);
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder(@"rest/api/2/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };

        // 2do! annotate the request.CustomFieldNNN with JSON field name based on ICTime server metadata (retrieved previously)
        var request = new api.rest.request.ICTimeAddWorklogByIssueKey(
            IssueKey: issueKey,
            Started: day
                .ToDateTime(TimeOnly.MinValue)
                .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
                .Replace(":", string.Empty)
                .Replace(';', ':'),
            TimeSpentSeconds: timeSpentSeconds,
            Activity: string.IsNullOrEmpty(activity) ? null : int.Parse(activity),
            Comment: comment
        );

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery, request);
        await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
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
        await _vanillaJiraApi.DeleteWorklog(issueId, worklogId, notifyUsers);
    }

    public async Task UpdateWorklog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        await _vanillaJiraApi.UpdateWorklog(issueKey, worklogId, day, timeSpentSeconds, activity, comment);
    }
}
