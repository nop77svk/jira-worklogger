namespace jwl.jira.Flavours;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using jwl.infra;
using jwl.jira.api.rest.common;
using jwl.jira.api.rest.response;
using jwl.jira.Exceptions;

public class VanillaCloudV2JiraClient
    : IJiraClient
{
    private readonly HttpClient _httpClient;
    private readonly FlavourCloudV2Options? _flavourOptions;
    private Lazy<JiraUserInfo> _lazyUserInfo;

    public JiraUserInfo UserInfo => _lazyUserInfo.Value;
    public string UserName { get; }

    public VanillaCloudV2JiraClient(HttpClient httpClient, string userName, FlavourCloudV2Options? flavourOptions)
    {
        _httpClient = httpClient;
        UserName = userName;
        _lazyUserInfo = new Lazy<JiraUserInfo>(() => GetUserInfo().Result);
        _flavourOptions = flavourOptions;
    }

    public async Task AddWorkLog(string issueKey, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        string pluginBaseUri = _flavourOptions?.PluginBaseUri
            ?? throw new JiraClientException(nameof(IFlavourOptions.PluginBaseUri));

        DateTime dayDt = day.ToDateTime(TimeOnly.MinValue);

        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{pluginBaseUri}/issue")
                .Add(issueKey)
                .Add(@"worklog"),
            Query = new UriQueryBuilder()
                .Add(@"notifyUsers", "false")
                .Add(@"adjustEstimate", "auto")
        };

        StringBuilder commentBuilder = new StringBuilder();

        if (!string.IsNullOrEmpty(activity))
        {
            commentBuilder.Append($"({activity})");
        }

        if (!string.IsNullOrEmpty(comment) && commentBuilder.Length > 0)
        {
            commentBuilder.Append(Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(comment))
        {
            commentBuilder.Append(comment);
        }

        string dayFormatted = dayDt
            .ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz")
            .Replace(":", string.Empty)
            .Replace(';', ':');

        var request = new api.rest.request.JiraAddWorklogByIssueKey(
            Started: dayFormatted,
            TimeSpentSeconds: timeSpentSeconds,
            Comment: commentBuilder.ToString()
        );

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery.TrimStart('/'), request);
        }
        catch (Exception ex)
        {
            throw new AddWorkLogException(issueKey, dayDt, timeSpentSeconds, ex)
            {
                Activity = activity,
                Comment = comment
            };
        }

        await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
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
        string pluginBaseUri = _flavourOptions?.PluginBaseUri
            ?? throw new JiraClientException(nameof(IFlavourOptions.PluginBaseUri));

        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{pluginBaseUri}/issue")
                .Add(issueId.ToString())
                .Add(@"worklog")
                .Add(worklogId.ToString()),
            Query = new UriQueryBuilder()
                .Add(@"notifyUsers", notifyUsers.ToString().ToLower())
                .Add(@"adjustEstimate", "auto")
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.DeleteAsync(uriBuilder.Uri.PathAndQuery.TrimStart('/'));
        }
        catch (Exception ex)
        {
            throw new DeleteWorklogException(issueId, worklogId, ex);
        }

        await VanillaJiraClient.CheckHttpResponseForErrorMessages(response);
    }

    public async Task<WorkLogType[]> GetAvailableActivities(string issueKey)
    {
        await Task.CompletedTask;
        return Array.Empty<WorkLogType>();
    }

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
        string pluginBaseUri = _flavourOptions?.PluginBaseUri
            ?? throw new JiraClientException(nameof(IFlavourOptions.PluginBaseUri));

        (DateTime minDt, DateTime supDt) = DateOnlyUtils.DateOnlyRangeToDateTimeRange(from, to);

        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{pluginBaseUri}/issue")
                .Add(issueKey)
                .Add(@"worklog"),
            Query = new UriQueryBuilder()
                .Add(@"startedAfter", minDt.ToUnixTimeStamp())
                .Add(@"startedBefore", supDt.ToUnixTimeStamp())
        };

        string uri = uriBuilder.Uri.PathAndQuery.TrimStart('/');

        JiraGetIssueWorklogsResponse? response;
        try
        {
            response = await _httpClient.GetAsJsonAsync<JiraGetIssueWorklogsResponse>(uri);
        }
        catch (Exception ex)
        {
            throw new GetIssueWorkLogsException(issueKey, from.ToDateTime(TimeOnly.MinValue), to.ToDateTime(TimeOnly.MinValue).AddDays(1), ex);
        }

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
        {
            return Array.Empty<WorkLog>();
        }

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

    public Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment) => throw new NotImplementedException();

    private async Task<CloudFindUsersResponseElement[]> FindUsers(string userName)
    {
        string pluginBaseUri = _flavourOptions?.PluginBaseUri
            ?? throw new JiraClientException(nameof(IFlavourOptions.PluginBaseUri));

        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = $"{pluginBaseUri}/user/search",
            Query = new UriQueryBuilder()
                .Add(@"query", UserName)
        };

        try
        {
            var result = await _httpClient.GetAsJsonAsync<CloudFindUsersResponseElement[]>(uriBuilder.Uri.PathAndQuery.TrimStart('/'));
            return result;
        }
        catch (Exception ex)
        {
            throw new JiraClientException($"Error finding users matching user name {UserName}", ex);
        }
    }

    private async Task<JiraUserInfo> GetUser(string accountId)
    {
        string pluginBaseUri = _flavourOptions?.PluginBaseUri
            ?? throw new JiraClientException(nameof(IFlavourOptions.PluginBaseUri));

        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = $"{pluginBaseUri}/user",
            Query = new UriQueryBuilder()
                .Add(@"accountId", accountId)
        };

        try
        {
            JiraUserInfo result = await _httpClient.GetAsJsonAsync<JiraUserInfo>(uriBuilder.Uri.PathAndQuery.TrimStart('/'));
            return result;
        }
        catch (Exception ex)
        {
            throw new JiraClientException($"Error retrieving user {UserName} info", ex);
        }
    }

    private async Task<JiraUserInfo> GetUserInfo()
    {
        CloudFindUsersResponseElement[] findUsersResult = await FindUsers(UserName);
        if (findUsersResult.Length == 0)
        {
            throw new JiraClientException($"No users found matching the user name {UserName}");
        }

        CloudFindUsersResponseElement firstUserFound = findUsersResult.FirstOrDefault(user => !string.IsNullOrEmpty(user.AccountId))
            ?? findUsersResult[0];

        string userAccountId = firstUserFound.AccountId
            ?? throw new JiraClientException($"Failed to retrieve accountId for the user {UserName}");

        JiraUserInfo result = await GetUser(userAccountId);
        return result;
    }
}
