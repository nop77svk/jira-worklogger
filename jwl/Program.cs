namespace jwl;
using System.Net;
using NoP77svk.Console;

internal class Program
{
    internal static void Main(string[] args)
    {
        Config config = new Config();

        Console.Error.Write($"Enter password for {config.ServerConfig.JiraUserName}: ");
        string jiraPassword;
        if (string.IsNullOrEmpty(config.ServerConfig.JiraUserPassword))
        {
            jiraPassword = SecretConsoleExt.ReadLineInSecret(_ => '*', true);
        }
        else
        {
            jiraPassword = config.ServerConfig.JiraUserPassword;
        }

        CredentialCache credentialCache = new CredentialCache();
        credentialCache.Add(new Uri(config.ServerConfig.BaseUrl), "Basic", new NetworkCredential(config.ServerConfig.JiraUserName, jiraPassword));

        using HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            UseProxy = config.ServerConfig.UseProxy,
            UseDefaultCredentials = false,
            Credentials = credentialCache,
            MaxConnectionsPerServer = config.ServerConfig.MaxConnectionsPerServer
        };
        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => config.ServerConfig.SkipSslCertificateCheck;
        using HttpClient httpClient = new HttpClient(httpClientHandler);
    }
}
