namespace jwl;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using NoP77svk.Console;
using NoP77svk.Linq;
using ShellProgressBar;

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

        Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> availableWorklogTypes = await GetAvailableWorklogTypesDictionary(jiraClient);
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
//            .Where(issueKey => !issueKey.StartsWith("ADMIN-")) // 2do! remove
            .Distinct()
            .OrderByDescending(x => x)
            .ToArray();
        DateTime[] inputWorklogDays = worklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();
        DateTime minInputWorklogDay = inputWorklogDays.First().Date;
        DateTime supInputWorklogDay = inputWorklogDays.Last().Date.AddDays(1);

        using ProgressBar worklogsToBeDeletedProgress = new ProgressBar(0, "Retrieving list of worklogs to be deleted");
        (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] currentIssueWorklogs = await RetrieveWorklogsForDeletion(jiraClient, inputIssueKeys, config.ServerConfig.JiraUserName, minInputWorklogDay, supInputWorklogDay, worklogsToBeDeletedProgress);
        Console.Out.WriteLine($"There are {currentIssueWorklogs.Length} workglogs to be deleted");
    }

    private static async Task<(string, jira.api.rest.response.JiraIssueWorklogsWorklog)[]> RetrieveWorklogsForDeletion(
        JiraServerApi jiraClient,
        IEnumerable<string> issueKeys,
        string authorUserName,
        DateTime minWorklogDay,
        DateTime supWorklogDay,
        IProgressBar progressBar
    )
    {
        (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] result;

        Dictionary<string, Task<jira.api.rest.response.JiraIssueWorklogs>> currentIssueWorklogsTasks = issueKeys
            .ToDictionary(
                keySelector: issueKey => issueKey,
                elementSelector: issueKey => jiraClient.GetIssueWorklogs(issueKey)
            );
        progressBar.MaxTicks += currentIssueWorklogsTasks.Count + 1;

        await MultiTask.WhenAll(
            tasks: currentIssueWorklogsTasks.Select(x => x.Value),
            reportProgress: (progress, _) => progressBar.Tick(progress.DoneSoFar, progress.ErredSoFar > 0 ? $"({progress.ErredSoFar} errors thus far)" : null)
        );

        result = currentIssueWorklogsTasks
            .Unnest(
                retrieveNestedCollection: x => x.Value.Result.Worklogs
                    .Where(worklog => worklog.Author.Name.Equals(authorUserName))
                    .Where(worklog => worklog.Created.Value >= minWorklogDay && worklog.Created.Value < supWorklogDay),
                resultSelector: (outer, inner) => new ValueTuple<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
            )
            .ToArray();

        progressBar.Tick();
        return result;
    }

    private static async Task<Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue>> GetAvailableWorklogTypesDictionary(JiraServerApi jiraClient)
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
