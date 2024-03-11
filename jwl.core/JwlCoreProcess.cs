namespace jwl.core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using jwl.jira.Flavours;
using NoP77svk.Linq;

public class JwlCoreProcess : IDisposable
{
    public const int TotalProcessSteps = 7;

    public static Version? ExeVersion => AssemblyVersioning.GetExeVersion();
    public static Version? CoreVersion => AssemblyVersioning.GetCoreVersion(typeof(JwlCoreProcess));

    public ICoreProcessFeedback? Feedback { get; init; }
    public ICoreProcessInteraction Interaction { get; }

    private bool _isDisposed;

    private AppConfig _config;
    private IJiraClient _jiraClient;
    private HttpClient _httpClient => _httpClientFactory.HttpClient;
    private readonly ConfigDrivenHttpClientFactory _httpClientFactory;

    public JwlCoreProcess(AppConfig config, ICoreProcessInteraction interaction)
    {
        _config = config;
        Interaction = interaction;

        RestrictThreadPoolToMaxNumberOfConnections();

        _httpClientFactory = new ConfigDrivenHttpClientFactory(config);

        string userName = _config.User?.Name
            ?? throw new ArgumentNullException($"{nameof(_config)}.{nameof(_config.User)}.{nameof(_config.User.Name)})");
        _jiraClient = ServerApiFactory.CreateApi(_httpClient, userName, _config.JiraServer);

        // 2do! optional trace-logging the HTTP requests
        // 2do! optional trace-logging the HTTP responses
    }

#pragma warning disable CS1998
    public async Task PreProcess()
    {
        Feedback?.OverallProcessStart();

        string? jiraUserName = _config.User?.Name;
        string? jiraUserPassword = _config.User?.Password;

        if (string.IsNullOrEmpty(jiraUserName) || string.IsNullOrEmpty(jiraUserPassword))
        {
            if (Interaction != null)
                (jiraUserName, jiraUserPassword) = Interaction.AskForCredentials(jiraUserName);
        }

        if (string.IsNullOrEmpty(jiraUserName) || string.IsNullOrEmpty(jiraUserPassword))
            throw new ArgumentNullException($"Jira credentials not supplied");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(jiraUserName + ":" + jiraUserPassword)));
    }
    #pragma warning restore

    public async Task Process(IEnumerable<string> inputFiles)
    {
        string[] inputFilesFetched = inputFiles.ToArray();

        if (inputFilesFetched.Length > 0)
        {
            Feedback?.ReadCsvInputStart();
            InputWorkLog[] inputWorklogs = await ReadInputFiles(inputFiles);
            Feedback?.ReadCsvInputEnd();

            if (inputWorklogs.Length > 0)
            {
                Feedback?.RetrieveWorklogsForDeletionStart();
                WorkLog[] worklogsForDeletion = await RetrieveWorklogsForDeletion(inputWorklogs);
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

            _isDisposed = true;
        }
    }

    private void RestrictThreadPoolToMaxNumberOfConnections()
    {
        // 2do! Not sure if this is right...
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIoThreads);
        maxWorkerThreads = Math.Min(maxWorkerThreads, _config.JiraServer?.MaxConnectionsPerServer ?? maxWorkerThreads);
        maxIoThreads = Math.Min(maxIoThreads, _config.JiraServer?.MaxConnectionsPerServer ?? maxIoThreads);
        ThreadPool.SetMaxThreads(maxWorkerThreads, maxIoThreads);
    }

    private async Task FillJiraWithWorklogs(InputWorkLog[] inputWorklogs, WorkLog[] worklogsForDeletion)
    {
        Feedback?.FillJiraWithWorklogsSetTarget(inputWorklogs.Length, worklogsForDeletion.Length);

        if (_jiraClient.UserInfo?.Key is null)
            throw new ArgumentNullException(@"Unresolved Jira key for the logged-on user");

        Task[] fillJiraWithWorklogsTasks = worklogsForDeletion
            .Select(worklog => _jiraClient.DeleteWorkLog(worklog.IssueId, worklog.Id))
            .Concat(inputWorklogs
                .Select(x => _jiraClient.AddWorkLog(
                    issueKey: x.IssueKey.ToString(),
                    day: DateOnly.FromDateTime(x.Date),
                    timeSpentSeconds: (int)x.TimeSpent.TotalSeconds,
                    activity: x.WorkLogActivity,
                    comment: x.WorkLogComment
                ))
            )
            .ToArray();

        MultiTaskStats progress = new MultiTaskStats(fillJiraWithWorklogsTasks.Length);
        MultiTask multiTask = new MultiTask()
        {
            OnTaskAwaited = t => Feedback?.FillJiraWithWorklogsProcess(progress.ApplyTaskStatus(t.Status))
        };

        await multiTask.WhenAll(fillJiraWithWorklogsTasks);
    }

    private async Task<InputWorkLog[]> ReadInputFiles(IEnumerable<string> fileNames)
    {
        Task<InputWorkLog[]>[] readerTasks = fileNames
            .Select(fileName => ReadInputFile(fileName))
            .ToArray();

        Feedback?.ReadCsvInputSetTarget(readerTasks.Length);

        MultiTaskStats progressStats = new MultiTaskStats(readerTasks.Length);
        MultiTask multiTask = new MultiTask()
        {
            OnTaskAwaited = t => Feedback?.ReadCsvInputProcess(progressStats.ApplyTaskStatus(t.Status))
        };

        if (readerTasks.Any())
            await multiTask.WhenAll(readerTasks);

        InputWorkLog[] result = readerTasks
            .SelectMany(response => response.Result)
            .ToArray();

        return result;
    }

    private async Task<InputWorkLog[]> ReadInputFile(string fileName)
    {
        WorklogReaderAggregatedConfig readerConfig = new WorklogReaderAggregatedConfig()
        {
            CsvFormatConfig = _config.CsvOptions
        };
        using IWorklogReader worklogReader = WorklogReaderFactory.GetReaderFromFilePath(fileName, readerConfig);

        return await Task.Factory.StartNew(() => worklogReader.Read().ToArray());
    }

    private async Task<WorkLog[]> RetrieveWorklogsForDeletion(InputWorkLog[] inputWorklogs)
    {
        WorkLog[] result;

        if (string.IsNullOrEmpty(_jiraClient.UserInfo?.Key))
            throw new ArgumentNullException(@"Empty user key preloaded from Jira server");

        DateTime[] inputWorklogDays = inputWorklogs
            .Select(worklog => worklog.Date)
            .OrderBy(worklogDate => worklogDate)
            .ToArray();

        if (!inputWorklogDays.Any())
        {
            result = Array.Empty<WorkLog>();
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

            result = await _jiraClient.GetIssueWorkLogs(minInputWorklogDay, maxInputWorklogDay, inputIssueKeys);
        }

        return result;
    }
}
