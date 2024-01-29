namespace jwl.jira;
using jwl.jira.api.rest.common;
using System.Text;

// https://interconcept.atlassian.net/wiki/spaces/ICTIME/pages/31686672/API
// https://interconcept.atlassian.net/wiki/spaces/ICBIZ/pages/34701333/REST+Services
public class JiraWithICTimePluginMock
    : IJiraClient
{
    public string UserName { get; }

    private readonly HttpClient _httpClient;

    public JiraWithICTimePluginMock(HttpClient httpClient, string userName)
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

    public async Task<WorkLogType[]> GetWorklogTypes()
    {
        return await _vanillaJiraApi.GetWorklogTypes();
    }

    public async Task<WorkLog[]> GetIssueWorklogs(DateOnly from, DateOnly to, IEnumerable<string>? issueKeys)
    {
        return await _vanillaJiraApi.GetIssueWorklogs(from, to, issueKeys);
    }

    public async Task AddWorklog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        StringBuilder commentBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(activity))
            commentBuilder.Append($"({activity}){Environment.NewLine}");
        commentBuilder.Append(comment);

        await _vanillaJiraApi.AddWorklog(issueKey, day, timeSpentSeconds, null, commentBuilder.ToString());
    }

    public async Task AddWorklogPeriod(string issueKey, DateOnly dayFrom, DateOnly dayTo, int timeSpentSeconds, string? activity, string? comment, bool includeNonWorkingDays = false)
    {
        StringBuilder commentBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(activity))
            commentBuilder.Append($"({activity}){Environment.NewLine}");
        commentBuilder.Append(comment);

        await _vanillaJiraApi.AddWorklogPeriod(issueKey, dayFrom, dayTo, timeSpentSeconds, null, commentBuilder.ToString(), includeNonWorkingDays);
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
