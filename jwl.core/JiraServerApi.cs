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
            .AddResourceFolder(Uri.EscapeDataString(issueKey))
            .AddResourceFolder(@"worklog");

        IAsyncEnumerable<api.rest.response.JiraIssueWorklogs> response = _wsClient.EndpointGetObject<api.rest.response.JiraIssueWorklogs>(wsep);

        return await response.FirstAsync();
    }
}
