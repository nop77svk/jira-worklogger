namespace jwl;
using System.Net.Http.Headers;
using System.Text;
using jwl.jira;
using jwl.jira.api.rest.common;
using jwl.inputs;
using NoP77svk.Console;
using NoP77svk.Linq;

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

        using HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            UseProxy = config.ServerConfig.UseProxy,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = config.ServerConfig.MaxConnectionsPerServer
        };

        if (config.ServerConfig.SkipSslCertificateCheck)
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using HttpClient httpClient = new HttpClient(httpClientHandler);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ServerConfig.JiraUserName + ":" + jiraPassword)));

        JiraServerApi jiraClient = new JiraServerApi(httpClient, config.ServerConfig.BaseUrl);

        Dictionary<string, TempoWorklogAttributeStaticListValue> availableWorklogTypes = await GetAvailableWorklogTypesDictionary(jiraClient);
        Console.Out.WriteLine($"There are {availableWorklogTypes.Count} worklog types available on the server");

        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(@"d:\x.csv");
        JiraWorklog[] worklogs = worklogReader
            .Read(row =>
            {
                if (!availableWorklogTypes.ContainsKey(row.TempWorklogType))
                    throw new InvalidDataException($"Worklog type {row.TempWorklogType} not found on server");
            })
            .ToArray();

        Console.Out.WriteLine($"There are {worklogs.Length} worklogs on input");

        string[] inputIssueKeys = worklogs
            .Select(worklog => worklog.IssueKey.ToString())
            .Where(issueKey => !issueKey.StartsWith("ADMIN-")) // 2do! remove
            .Distinct()
            .ToArray();
        DateTime[] inputWorklogDays = worklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();
        DateTime minInputWorklogDay = inputWorklogDays.First().Date;
        DateTime supInputWorklogDay = inputWorklogDays.Last().Date.AddDays(1);

        // retrieve worklogs for the current issues and days
        Dictionary<string, Task<jira.api.rest.response.JiraIssueWorklogs>> currentIssueWorklogsTasks = inputIssueKeys
            .ToDictionary(
                keySelector: issueKey => issueKey,
                elementSelector: issueKey => jiraClient.GetIssueWorklogs(issueKey)
            );
        await Task.WhenAll(currentIssueWorklogsTasks.Select(x => x.Value));
        var currentIssueWorklogs = currentIssueWorklogsTasks
            .Unnest(
                retrieveNestedCollection: x => x.Value.Result.Worklogs
                    .Where(worklog => worklog.Author.Name.Equals(config.ServerConfig.JiraUserName))
                    .Where(worklog => worklog.Created.Value >= minInputWorklogDay && worklog.Created.Value < supInputWorklogDay),
                resultSelector: (outer, inner) => new KeyValuePair<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
            )
            .ToArray();

        foreach (var clog in currentIssueWorklogs)
        {
            Console.Out.WriteLine($"Jira issue {clog.Key}, id {clog.Value.IssueId} worklog id {clog.Value.Id}"); //  by {clog.Value.Author.Name} ({clog.Value.Author.Key})
        }

        Console.ReadKey();
    }

    private static async Task<Dictionary<string, TempoWorklogAttributeStaticListValue>> GetAvailableWorklogTypesDictionary(JiraServerApi jiraClient)
    {
        Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> result;

        IEnumerable<jira.api.rest.response.TempoWorklogAttributeDefinition> attrEnumDefs = await jiraClient.GetWorklogAttributesEnum();
        result = attrEnumDefs
            .Where(attrDef => attrDef.Key == TempoTimesheetsPluginApiExt.WorklogTypeAttributeKey)
            .Where(attrDef => attrDef.Type.Value == jira.api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList)
            .Unnest(
                retrieveNestedCollection: attrDef => attrDef.StaticListValues ?? Array.Empty<jira.api.rest.common.TempoWorklogAttributeStaticListValue>(),
                resultSelector: (outer, inner) => inner
            )
            .ToDictionary(staticListItem => staticListItem.Value);

        return result;
    }
}
