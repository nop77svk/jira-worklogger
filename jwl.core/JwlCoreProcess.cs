namespace jwl.core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using jwl.jira.api.rest.response;
using Microsoft.Extensions.Configuration;
using NoP77svk.Linq;

public class JwlCoreProcess : IDisposable
{
    public const int TotalProcessSteps = 7;

    public ICoreProcessFeedback? Feedback { get; init; }
    public ICoreProcessInteraction _interaction { get; }

    private bool _isDisposed;

    private AppConfig _config;
    private HttpClientHandler _httpClientHandler;
    private HttpClient _httpClient;
    private JiraServerApi _jiraClient;

    private jwl.jira.api.rest.common.JiraUserInfo? _userInfo;
    private Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> availableWorklogTypes = new ();

    public JwlCoreProcess(AppConfig config, ICoreProcessInteraction interaction)
    {
        _config = config;
        _interaction = interaction;

        _httpClientHandler = new HttpClientHandler()
        {
            UseProxy = _config.JiraServer?.UseProxy ?? false,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = _config.JiraServer?.MaxConnectionsPerServer ?? AppConfigFactory.DefaultMaxConnectionsPerServer
        };

        if (_config.JiraServer?.SkipSslCertificateCheck ?? false)
            _httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        _httpClient = new HttpClient(_httpClientHandler);
        _jiraClient = new JiraServerApi(_httpClient, _config.JiraServer?.BaseUrl ?? string.Empty);

        /* 2do!...
        _jiraClient.WsClient.HttpRequestPostprocess = req =>
        {
            // 2do! optional logging of request bodies
        };

        _jiraClient.WsClient.HttpResponsePostprocess = resp =>
        {
            // 2do! optional logging of response bodies
        };
        */
    }

    public async Task PreProcess()
    {
        Feedback?.OverallProcessStart();

        string? jiraUserName = _config.User?.Name;
        string? jiraUserPassword = _config.User?.Password;

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
        string[] inputFilesFetched = inputFiles.ToArray();

        if (inputFilesFetched.Length > 0)
        {
            Feedback?.ReadCsvInputStart();
            JiraWorklog[] inputWorklogs = await ReadInputFiles(inputFiles);
            Feedback?.ReadCsvInputEnd();

            if (inputWorklogs.Length > 0)
            {
                Feedback?.RetrieveWorklogsForDeletionStart();
                jira.api.rest.response.TempoWorklog[] worklogsForDeletion = await RetrieveWorklogsForDeletion(inputWorklogs);
                Feedback?.RetrieveWorklogsForDeletionEnd();

                Feedback?.FillJiraWithWorklogsStart();
                await FillJiraWithWorklogs(inputWorklogs, worklogsForDeletion);
                Feedback?.FillJiraWithWorklogsEnd();
            }
            else
            {
                Feedback?.NoWorklogsToFill();
            }

            Feedback?.OverallProcessEnd();
        }
        else
        {
            Feedback?.NoFilesOnInput();
        }
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
            _httpClient?.Dispose();
            _httpClientHandler?.Dispose();

            _isDisposed = true;
        }
    }

    private async Task FillJiraWithWorklogs(JiraWorklog[] inputWorklogs, TempoWorklog[] worklogsForDeletion)
    {
        Feedback?.FillJiraWithWorklogsSetTarget(inputWorklogs.Length, worklogsForDeletion.Length);

        if (_userInfo == null || _userInfo.Key == null)
            throw new ArgumentNullException(@"Unresolved Jira key for the logged-on user");

        Task[] fillJiraWithWorklogsTasks = worklogsForDeletion
            .Select(worklog => _jiraClient.DeleteWorklog(worklog.Id ?? 0))
            .Concat(inputWorklogs
                .Select(worklog => _jiraClient.AddWorklog(worklog.IssueKey.ToString(), _userInfo.Key, DateOnly.FromDateTime(worklog.Date), (int)worklog.TimeSpent.TotalSeconds, worklog.TempWorklogType, string.Empty))
            )
            .ToArray();

        MultiTaskProgress progress = new MultiTaskProgress(fillJiraWithWorklogsTasks.Length);

        await MultiTask.WhenAll(
            tasks: fillJiraWithWorklogsTasks,
            progressFeedback: (_, t) => Feedback?.FillJiraWithWorklogsProcess(progress.AddTaskStatus(t?.Status))
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

        MultiTaskProgress progress = new MultiTaskProgress(readerTasks.Length);

        if (readerTasks.Any())
        {
            await MultiTask.WhenAll(
                tasks: readerTasks,
                progressFeedback: (_, t) => Feedback?.ReadCsvInputProcess(progress.AddTaskStatus(t?.Status))
            );
        }

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
        WorklogReaderAggregatedConfig readerConfig = new WorklogReaderAggregatedConfig()
        {
            CsvFormatConfig = _config.CsvOptions
        };

        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(fileName, readerConfig);

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

    private async Task<jira.api.rest.response.TempoWorklog[]> RetrieveWorklogsForDeletion(JiraWorklog[] inputWorklogs)
    {
        jira.api.rest.response.TempoWorklog[] result;

        if (_userInfo == null)
            throw new ArgumentNullException(@"User info not preloaded from Jira server");
        if (string.IsNullOrEmpty(_userInfo.Key))
            throw new ArgumentNullException(@"Empty user key preloaded from Jira server");

        DateTime[] inputWorklogDays = inputWorklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();

        if (!inputWorklogDays.Any())
        {
            result = Array.Empty<jira.api.rest.response.TempoWorklog>();
        }
        else
        {
            DateOnly minInputWorklogDay = DateOnly.FromDateTime(inputWorklogDays.First().Date);
            DateOnly maxInputWorklogDay = DateOnly.FromDateTime(inputWorklogDays.Last().Date);

            string[] inputIssueKeys = inputWorklogs
                .Select(worklog => worklog.IssueKey.ToString())
                .Distinct()
                .OrderByDescending(x => x)
                .ToArray();

            result = await _jiraClient.GetIssueWorklogs(minInputWorklogDay, maxInputWorklogDay, inputIssueKeys, new string[] { _userInfo.Key });
        }

        return result;
    }
}
