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
                (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] worklogsForDeletion = await RetrieveWorklogsForDeletion(inputWorklogs);
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

    private async Task FillJiraWithWorklogs(JiraWorklog[] inputWorklogs, (string, JiraIssueWorklogsWorklog)[] worklogsForDeletion)
    {
        Feedback?.FillJiraWithWorklogsSetTarget(inputWorklogs.Length, worklogsForDeletion.Length);

        if (_userInfo == null || _userInfo.Key == null)
            throw new ArgumentNullException(@"Unresolved Jira key for the logged-on user");

        Task[] fillJiraWithWorklogsTasks = inputWorklogs
            .Select(worklog => _jiraClient.AddWorklog(worklog.IssueKey.ToString(), _userInfo.Key, DateOnly.FromDateTime(worklog.Date), (int)worklog.TimeSpent.TotalSeconds, worklog.TempWorklogType, string.Empty))
            .Concat(worklogsForDeletion
                .Select(worklog => _jiraClient.DeleteWorklog(worklog.Item2.IssueId.Value, worklog.Item2.Id.Value))
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

    private async Task<(string, jira.api.rest.response.JiraIssueWorklogsWorklog)[]> RetrieveWorklogsForDeletion(JiraWorklog[] inputWorklogs)
    {
        (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] result;

        if (_userInfo == null)
            throw new ArgumentNullException(@"User info not preloaded from Jira server");

        DateTime[] inputWorklogDays = inputWorklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();

        if (!inputWorklogDays.Any())
        {
            result = Array.Empty<(string, jira.api.rest.response.JiraIssueWorklogsWorklog)>();
        }
        else
        {
            DateTime minInputWorklogDay = inputWorklogDays.First().Date;
            DateTime supInputWorklogDay = inputWorklogDays.Last().Date.AddDays(1);

            string[] inputIssueKeys = inputWorklogs
                .Select(worklog => worklog.IssueKey.ToString())
                .Distinct()
                .OrderByDescending(x => x)
                .ToArray();

            Dictionary<string, Task<jira.api.rest.response.JiraIssueWorklogs>> currentIssueWorklogsTasks = inputIssueKeys
                .ToDictionary(
                    keySelector: issueKey => issueKey,
                    elementSelector: issueKey => _jiraClient.GetIssueWorklogs(issueKey)
                );

            Feedback?.RetrieveWorklogsForDeletionSetTarget(currentIssueWorklogsTasks.Count);

            MultiTaskProgress progress = new MultiTaskProgress(currentIssueWorklogsTasks.Count);

            await MultiTask.WhenAll(
                tasks: currentIssueWorklogsTasks.Select(x => x.Value),
                progressFeedback: (_, t) => Feedback?.RetrieveWorklogsForDeletionProcess(progress.AddTaskStatus(t?.Status))
            );

            return currentIssueWorklogsTasks
                .Unnest(
                    retrieveNestedCollection: x => (x.Value.Result.Worklogs ?? Array.Empty<JiraIssueWorklogsWorklog>())
                        .Where(worklog => worklog != null
                            && worklog.Author != null
                            && !string.IsNullOrEmpty(worklog.Author.Name)
                            && worklog.Author.Name.Equals(_userInfo.Name, StringComparison.OrdinalIgnoreCase)
                        )
                        .Where(worklog => worklog != null
                            && worklog.Started.Value >= minInputWorklogDay
                            && worklog.Started.Value < supInputWorklogDay
                        ),
                    resultSelector: (outer, inner) => new ValueTuple<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
                )
                .ToArray();
        }

        return result;
    }
}
