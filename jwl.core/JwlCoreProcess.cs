namespace jwl.core;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using jwl.jira.api.rest.response;
using NoP77svk.Linq;
using NoP77svk.Web.WS;

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

        string jiraPassword;
        if (string.IsNullOrEmpty(_config.UserConfig.JiraUserPassword))
            jiraPassword = _interaction?.AskForPassword(_config.UserConfig.JiraUserName) ?? string.Empty;
        else
            jiraPassword = _config.UserConfig.JiraUserPassword;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.UserConfig.JiraUserName + ":" + jiraPassword)));

        Feedback?.PreloadAvailableWorklogTypesStart();
        availableWorklogTypes = await PreloadAvailableWorklogTypes();
        Feedback?.PreloadAvailableWorklogTypesEnd();

        Feedback?.PreloadUserInfoStart(_config.UserConfig.JiraUserName);
        _userInfo = await _jiraClient.GetUserInfo(_config.UserConfig.JiraUserName);
        Feedback?.PreloadUserInfoEnd();
    }

    public async Task Process(string inputFile)
    {
        Feedback?.ReadCsvInputStart();
        JiraWorklog[] inputWorklogs = await ReadInputFile(inputFile);
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
            .Where(worklog => worklog.Item2.IssueId != null && worklog.Item2.Id != null)
            .Select(worklog => _jiraClient.DeleteWorklog(worklog.Item2.IssueId?.Value ?? 0, worklog.Item2.Id?.Value ?? 0))
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

    private async Task<JiraWorklog[]> ReadInputFile(string fileName)
    {
        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(fileName);

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
                        && worklog.Started != null
                        && worklog.Started?.Value >= minInputWorklogDay
                        && worklog.Started?.Value < supInputWorklogDay
                    ),
                resultSelector: (outer, inner) => new ValueTuple<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
            )
            .ToArray();
    }
}
