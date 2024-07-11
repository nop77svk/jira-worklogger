namespace jwl.jira;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Xml;
using jwl.infra;
using jwl.jira.api.rest.response;
using jwl.jira.Exceptions;
using jwl.jira.Flavours;

public class VanillaJiraClient
    : IJiraClient
{
    public string UserName { get; }
    public api.rest.common.JiraUserInfo UserInfo => _lazyUserInfo.Value;

    private readonly HttpClient _httpClient;
    private readonly Lazy<jwl.jira.api.rest.common.JiraUserInfo> _lazyUserInfo;
    private readonly FlavourVanillaJiraOptions _flavourOptions;

    public VanillaJiraClient(HttpClient httpClient, string userName, FlavourVanillaJiraOptions? flavourOptions)
    {
        _httpClient = httpClient;
        UserName = userName;
        _lazyUserInfo = new Lazy<api.rest.common.JiraUserInfo>(() => GetUserInfo().Result);
        _flavourOptions = flavourOptions ?? new FlavourVanillaJiraOptions();
    }

    public static async Task CheckHttpResponseForErrorMessages(HttpResponseMessage responseMessage)
    {
        using Stream responseContentStream = await responseMessage.Content.ReadAsStreamAsync();

        if (responseContentStream.Length > 0)
        {
            try
            {
                JiraRestResponse jsonResponseContent = await HttpClientExt.DeserializeJsonStreamAsync<JiraRestResponse>(responseContentStream);

                if (jsonResponseContent.ErrorMessages?.Any() ?? false)
                    throw new InvalidOperationException(string.Join(Environment.NewLine, jsonResponseContent.ErrorMessages));
            }
            catch (JsonException jsonEx)
            {
                try
                {
                    responseContentStream.Seek(0, SeekOrigin.Begin);
                    ICTimeXmlResponse xmlResponseContent = await HttpClientExt.DeserializeXmlStreamAsync<ICTimeXmlResponse>(responseContentStream);

                    if (xmlResponseContent.Success == null)
                        throw new InvalidOperationException(await responseMessage.Content.ReadAsStringAsync());
                }
                catch (XmlException xmlEx)
                {
                    throw new InvalidOperationException("Cannot deserialize HTTP response to any of the recognized structures", new AggregateException(jsonEx, xmlEx));
                }
            }
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
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/issue")
                .Add(issueKey)
                .Add(@"worklog")
        };

        string uri = uriBuilder.Uri.PathAndQuery.TrimStart('/');

        JiraIssueWorklogs? response;
        try
        {
            response = await _httpClient.GetAsJsonAsync<JiraIssueWorklogs>(uri);
        }
        catch (Exception ex)
        {
            throw new GetIssueWorkLogsException(issueKey, from.ToDateTime(TimeOnly.MinValue), to.ToDateTime(TimeOnly.MinValue).AddDays(1), ex);
        }

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
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/issue")
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

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(uriBuilder.Uri.PathAndQuery.TrimStart('/'), request);
        }
        catch (Exception ex)
        {
            throw new AddWorkLogException(issueKey, day.ToDateTime(TimeOnly.MinValue), timeSpentSeconds, ex)
            {
                Activity = activity,
                Comment = comment
            };
        }

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
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/issue")
                .Add(issueId.ToString())
                .Add(@"worklog")
                .Add(worklogId.ToString()),
            Query = new UriQueryBuilder()
                .Add(@"notifyUsers", notifyUsers.ToString().ToLower())
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

        await CheckHttpResponseForErrorMessages(response);
    }

    public async Task UpdateWorkLog(string issueKey, long worklogId, DateOnly day, int timeSpentSeconds, string? activity, string? comment)
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = new UriPathBuilder($"{_flavourOptions.PluginBaseUri}/issue")
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

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PutAsJsonAsync(uriBuilder.Uri.PathAndQuery.TrimStart('/'), request);
        }
        catch (Exception ex)
        {
            throw new UpdateWorklogException(issueKey, worklogId, day.ToDateTime(TimeOnly.MinValue), timeSpentSeconds, ex)
            {
                Activity = activity,
                Comment = comment
            };
        }

        await CheckHttpResponseForErrorMessages(response);
    }

    private async Task<api.rest.common.JiraUserInfo> GetUserInfo()
    {
        UriBuilder uriBuilder = new UriBuilder()
        {
            Path = $"{_flavourOptions.PluginBaseUri}/user",
            Query = new UriQueryBuilder()
                .Add(@"username", UserName)
        };

        try
        {
            return await _httpClient.GetAsJsonAsync<api.rest.common.JiraUserInfo>(uriBuilder.Uri.PathAndQuery.TrimStart('/'));
        }
        catch (Exception ex)
        {
            throw new JiraClientException($"Error retrieving user {UserName} info", ex);
        }
    }
}
