namespace jwl.core;
using System.Linq;
using System.Net.Http;
using NoP77svk.Web.WS;

public class JiraServerApi
{
    public Uri BaseUrl { get; }

    private HttpClient _httpClient;
    private HttpWebServiceClient _wsClient;

    public JiraServerApi(HttpClient httpClient, string baseUrl)
    {
        BaseUrl = new Uri(baseUrl, UriKind.Absolute);
        _httpClient = httpClient;
        _wsClient = new HttpWebServiceClient(_httpClient, BaseUrl.Host, BaseUrl.Port, BaseUrl.Scheme);
    }

    public async Task<api.rest.common.JiraUserInfo> GetUserInfo(string userName)
    {
        IAsyncEnumerable<api.rest.common.JiraUserInfo> response = _wsClient.EndpointGetObject<api.rest.common.JiraUserInfo>(new JsonRestWsEndpoint(HttpMethod.Get)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"api")
            .AddResourceFolder(@"2")
            .AddResourceFolder(@"user")
            .AddQuery(@"username", userName)
        );

        return await response.FirstAsync();
    }

    public async Task<api.rest.response.JiraIssueWorklogs> GetIssueWorklogs(string issueKey)
    {
        JsonRestWsEndpoint wsep = new JsonRestWsEndpoint(HttpMethod.Get)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"api")
            .AddResourceFolder(@"2")
            .AddResourceFolder(issueKey)
            .AddResourceFolder(@"worklog");

        IAsyncEnumerable<api.rest.response.JiraIssueWorklogs> response = _wsClient.EndpointGetObject<api.rest.response.JiraIssueWorklogs>(wsep);

        return await response.FirstAsync();
    }

    public async Task DeleteWorklog(int issueId, int worklogId, bool notifyUsers = false)
    {
        await _wsClient.EndpointCall(new JsonRestWsEndpoint(HttpMethod.Delete)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"api")
            .AddResourceFolder(@"2")
            .AddResourceFolder(@"issue")
            .AddResourceFolder(issueId.ToString())
            .AddResourceFolder(@"worklog")
            .AddResourceFolder(worklogId.ToString())
            .AddQuery(@"notifyUsers", notifyUsers.ToString().ToLower())
        );
    }

    public async Task AddWorklog(string issueKey, DateTime day, int timeSpentSeconds, string comment)
    {
        var request = new api.rest.request.JiraAddWorklogByIssueKey()
        {
            Started = day.ToString(@"yyyy-MM-dd""T""hh"";""mm"";""ss.fffzzzz").Replace(":", string.Empty).Replace(';', ':'),
            TimeSpentSeconds = timeSpentSeconds,
            Comment = comment
        };

        await _wsClient.EndpointCall(new JsonRestWsEndpoint(HttpMethod.Post)
            .AddResourceFolder(@"rest")
            .AddResourceFolder(@"api")
            .AddResourceFolder(@"2")
            .AddResourceFolder(@"issue")
            .AddResourceFolder(issueKey)
            .AddResourceFolder(@"worklog")
            .WithContent(request)
        );
    }
}
