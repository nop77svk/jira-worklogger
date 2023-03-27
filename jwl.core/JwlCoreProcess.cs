namespace jwl.core;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using jwl.jira.api.rest.response;
using NoP77svk.Linq;

public class JwlCoreProcess : IDisposable
{
    public const int TotalProcessSteps = 7;

    public ICoreProcessFeedback? Feedback { get; init; }
    private ICoreProcessInteraction _interaction;

    private bool _isDisposed;

    private Config _config;
    private HttpClientHandler _httpClientHandler;
    private HttpClient _httpClient;
    private JiraServerApi _jiraClient;

    private jwl.jira.api.rest.common.JiraUserInfo? _userInfo;
    private Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> availableWorklogTypes = new ();

    public JwlCoreProcess(Config config, ICoreProcessInteraction interaction)
    {
        _config = config;
        _interaction = interaction;

        _httpClientHandler = new HttpClientHandler()
        {
            UseProxy = _config.ServerConfig.UseProxy,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = _config.ServerConfig.MaxConnectionsPerServer
        };

        if (_config.ServerConfig.SkipSslCertificateCheck)
            _httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        _httpClient = new HttpClient(_httpClientHandler);
        _jiraClient = new JiraServerApi(_httpClient, _config.ServerConfig.BaseUrl);

        _jiraClient.WsClient.HttpRequestPostprocess = req =>
        {
            // 2do! optional logging of request bodies
        };

        _jiraClient.WsClient.HttpResponsePostprocess = resp =>
        {
            // 2do! optional logging of response bodies
        };
    }

    public async Task PreProcess()
    {
        Feedback?.OverallProcessStart();

        string? jiraUserName = _config.UserConfig.JiraUserName;
        string? jiraUserPassword = _config.UserConfig.JiraUserPassword;

        if (string.IsNullOrEmpty(jiraUserName) || string.IsNullOrEmpty(jiraUserPassword))
        {
            if (_interaction != null)
                (jiraUserName, jiraUserPassword) = _interaction.AskForCredentials(jiraUserName);
        }

        if (string.IsNullOrEmpty(jiraUserName) || string.IsNullOrEmpty(jiraUserPassword))
            throw new ArgumentNullException($"Jira credentials not supplied");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(jiraUserName + ":" + jiraUserPassword)));

        Feedback?.PreloadAvailableWorklogTypesStart();
        availableWorklogTypes = await PreloadAvailableWorklogTypes();
        Feedback?.PreloadAvailableWorklogTypesEnd();

        Feedback?.PreloadUserInfoStart(jiraUserName);
        _userInfo = await _jiraClient.GetUserInfo(jiraUserName);
        Feedback?.PreloadUserInfoEnd();
    }

    public async Task Process(IEnumerable<string> inputFiles)
    {
        Feedback?.ReadCsvInputStart();
        JiraWorklog[] inputWorklogs = await ReadInputFiles(inputFiles);
        Feedback?.ReadCsvInputEnd();

        Feedback?.RetrieveWorklogsForDeletionStart();
        (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] worklogsForDeletion = await RetrieveWorklogsForDeletion(inputWorklogs);
        Feedback?.RetrieveWorklogsForDeletionEnd();

        Feedback?.DeleteExistingWorklogsStart();
        await DeleteExistingWorklogs(worklogsForDeletion);
        Feedback?.DeleteExistingWorklogsEnd();

        Feedback?.FillJiraWithWorklogsStart();
        await FillJiraWithWorklogs(inputWorklogs);
        Feedback?.FillJiraWithWorklogsEnd();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
            }

            // note: free unmanaged resources (unmanaged objects) and override finalizer
            // note: set large fields to null
            Feedback?.OverallProcessEnd();
            _httpClient?.Dispose();
            _httpClientHandler?.Dispose();

            _isDisposed = true;
        }
    }

    private async Task DeleteExistingWorklogs((string, JiraIssueWorklogsWorklog)[] worklogsForDeletion)
    {
        Feedback?.DeleteExistingWorklogsSetTarget(worklogsForDeletion.Length);

        Task[] deleteExistingWorklogsTasks = worklogsForDeletion
            .Select(worklog => _jiraClient.DeleteWorklog(worklog.Item2.IssueId.Value, worklog.Item2.Id.Value))
            .ToArray();

        await MultiTask.WhenAll(
            tasks: deleteExistingWorklogsTasks,
            reportProgress: (p, _) => Feedback?.DeleteExistingWorklogsProcess(p)
        );
    }

    private async Task FillJiraWithWorklogs(JiraWorklog[] inputWorklogs)
    {
        Feedback?.FillJiraWithWorklogsSetTarget(inputWorklogs.Length);

        if (_userInfo == null || _userInfo.Key == null)
            throw new ArgumentNullException(@"Unresolved Jira key for the logged-on user");

        Task[] fillJiraWithWorklogsTasks = inputWorklogs
            .Select(worklog => _jiraClient.AddWorklog(worklog.IssueKey.ToString(), _userInfo.Key, worklog.Date, (int)worklog.TimeSpent.TotalSeconds, worklog.TempWorklogType, string.Empty))
            .ToArray();

        await MultiTask.WhenAll(
            tasks: fillJiraWithWorklogsTasks,
            reportProgress: (p, _) => Feedback?.FillJiraWithWorklogsProcess(p)
        );
    }

    private async Task<Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue>> PreloadAvailableWorklogTypes()
    {
        IEnumerable<jira.api.rest.response.TempoWorklogAttributeDefinition> attrEnumDefs = await _jiraClient.GetWorklogAttributesEnum();

        return attrEnumDefs
            .Where(attrDef => attrDef.Key?.Equals(TempoTimesheetsPluginApiExt.WorklogTypeAttributeKey) ?? false)
            .Where(attrDef => attrDef.Type != null
                && attrDef.Type?.Value == jira.api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList
            )
            .Unnest(
                retrieveNestedCollection: attrDef => attrDef.StaticListValues ?? Array.Empty<jira.api.rest.common.TempoWorklogAttributeStaticListValue>(),
                resultSelector: (outer, inner) => inner
            )
            .Where(staticListItem => !string.IsNullOrEmpty(staticListItem.Value))
            .ToDictionary(staticListItem => staticListItem.Value ?? string.Empty);
    }

    private async Task<JiraWorklog[]> ReadInputFiles(IEnumerable<string> fileNames)
    {
        Task<JiraWorklog[]>[] readerTasks = fileNames
            .Select(fileName => ReadInputFile(fileName))
            .ToArray();

        Feedback?.ReadCsvInputSetTarget(readerTasks.Length);

        await MultiTask.WhenAll(readerTasks, (p, t) => Feedback?.ReadCsvInputProcess(p));

        JiraWorklog[] result = readerTasks
            .Unnest(
                retrieveNestedCollection: response => response.Result,
                resultSelector: (response, jiraWorklog) => jiraWorklog
            )
            .ToArray();

        return result;
    }

    private async Task<JiraWorklog[]> ReadInputFile(string fileName)
    {
        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(fileName, _config.CsvFormatConfig); // 2do! remove the dependency on CsvFormatConfig

        Task<JiraWorklog[]> response = Task.Factory.StartNew(() =>
        {
            return worklogReader
                .Read(row =>
                {
                    if (!availableWorklogTypes.ContainsKey(row.TempWorklogType))
                        throw new InvalidDataException($"Worklog type {row.TempWorklogType} not found on server");
                })
                .ToArray();
        });

        return await response;
    }

    private async Task<(string, jira.api.rest.response.JiraIssueWorklogsWorklog)[]> RetrieveWorklogsForDeletion(JiraWorklog[] inputWorklogs)
    {
        string[] inputIssueKeys = inputWorklogs
            .Select(worklog => worklog.IssueKey.ToString())
            .Distinct()
            .OrderByDescending(x => x)
            .ToArray();
        DateTime[] inputWorklogDays = inputWorklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();
        DateTime minInputWorklogDay = inputWorklogDays.First().Date;
        DateTime supInputWorklogDay = inputWorklogDays.Last().Date.AddDays(1);

        Dictionary<string, Task<jira.api.rest.response.JiraIssueWorklogs>> currentIssueWorklogsTasks = inputIssueKeys
            .ToDictionary(
                keySelector: issueKey => issueKey,
                elementSelector: issueKey => _jiraClient.GetIssueWorklogs(issueKey)
            );

        Feedback?.RetrieveWorklogsForDeletionSetTarget(currentIssueWorklogsTasks.Count + 1);

        await MultiTask.WhenAll(
            tasks: currentIssueWorklogsTasks.Select(x => x.Value),
            reportProgress: (progress, _) => Feedback?.RetrieveWorklogsForDeletionProcess(progress)
        );

        return currentIssueWorklogsTasks
            .Unnest(
                retrieveNestedCollection: x => (x.Value.Result.Worklogs ?? Array.Empty<JiraIssueWorklogsWorklog>())
                    .Where(worklog => worklog != null
                        && worklog.Author != null
                        && !string.IsNullOrEmpty(worklog.Author.Name)
                        && worklog.Author.Name.Equals(_config.UserConfig.JiraUserName, StringComparison.OrdinalIgnoreCase)
                    )
                    .Where(worklog => worklog != null
                        && worklog.Started.Value >= minInputWorklogDay
                        && worklog.Started.Value < supInputWorklogDay
                    ),
                resultSelector: (outer, inner) => new ValueTuple<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
            )
            .ToArray();
    }
}
