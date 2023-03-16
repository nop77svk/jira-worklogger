namespace jwl;
using System.Net;
using System.Text;
using jwl.core;
using jwl.inputs;
using NoP77svk.Console;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        Config config = new Config();

        string jiraPassword;
        if (string.IsNullOrEmpty(config.ServerConfig.JiraUserPassword))
        {
            Console.Error.Write($"Enter password for {config.ServerConfig.JiraUserName}: ");
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

        JiraServerApi jira = new JiraServerApi(httpClient, config.ServerConfig.BaseUrl);
        var attrEnum = await jira.GetWorklogAttributesEnum();
/*
        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(@"d:\x.csv");
        JiraWorklog[] worklogs = worklogReader.AsEnumerable().ToArray();
        Console.Out.WriteLine($"{worklogs.Length} lines on input");
*/
    }
}
