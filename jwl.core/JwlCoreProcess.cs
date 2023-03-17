namespace jwl.core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using jwl.infra;
using jwl.inputs;
using jwl.jira;
using NoP77svk.Linq;

public class JwlCoreProcess : IDisposable
{
    public const int TotalProcessSteps = 4;
    public Config _config { get; }

    private bool _isDisposed;
    private ICoreProcessFeedback _feedback;
    private ICoreProcessInteraction _interaction;

    private JiraServerApi? _jiraClient = null;

    public JwlCoreProcess(ICoreProcessFeedback feedback, ICoreProcessInteraction interaction)
    {
        _feedback = feedback;
        _interaction = interaction;
        _config = new Config();
    }

    public async Task Process(string inputFile)
    {
        _feedback.OverallProcessStart();

        using HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            UseProxy = _config.ServerConfig.UseProxy,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = _config.ServerConfig.MaxConnectionsPerServer
        };

        if (_config.ServerConfig.SkipSslCertificateCheck)
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using HttpClient httpClient = new HttpClient(httpClientHandler);

        string jiraPassword;
        if (string.IsNullOrEmpty(_config.UserConfig.JiraUserPassword))
            jiraPassword = _interaction.AskForPassword(_config.UserConfig.JiraUserName);
        else
            jiraPassword = _config.UserConfig.JiraUserPassword;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.UserConfig.JiraUserName + ":" + jiraPassword)));
        _jiraClient = new JiraServerApi(httpClient, _config.ServerConfig.BaseUrl);

        _feedback.PreloadAvailableWorklogTypesStart();
        Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> availableWorklogTypes = await PreloadAvailableWorklogTypes();
        _feedback.PreloadAvailableWorklogTypesEnd();

        _feedback.ReadCsvInputStart();
        JiraWorklog[] inputWorklogs = await ReadInputFile(inputFile, availableWorklogTypes);
        _feedback.ReadCsvInputEnd();

        _feedback.RetrieveWorklogsForDeletionStart();
        (string, jira.api.rest.response.JiraIssueWorklogsWorklog)[] worklogsForDeletion = await RetrieveWorklogsForDeletion(inputWorklogs);
        _feedback.RetrieveWorklogsForDeletionEnd();

        _feedback.OverallProcessEnd();
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
                _feedback.Dispose();
                _interaction.Dispose();
            }

            // note: free unmanaged resources (unmanaged objects) and override finalizer
            // note: set large fields to null
            _isDisposed = true;
        }
    }

    private async Task<JiraWorklog[]> ReadInputFile(string fileName, Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue> availableWorklogTypes)
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

    private async Task<Dictionary<string, jira.api.rest.common.TempoWorklogAttributeStaticListValue>> PreloadAvailableWorklogTypes()
    {
        if (_jiraClient == null)
            throw new ArgumentNullException("FATAL! Jira client not initialized");

        IEnumerable<jira.api.rest.response.TempoWorklogAttributeDefinition> attrEnumDefs = await _jiraClient.GetWorklogAttributesEnum();

        return attrEnumDefs
            .Where(attrDef => attrDef.Key == TempoTimesheetsPluginApiExt.WorklogTypeAttributeKey)
            .Where(attrDef => attrDef.Type.Value == jira.api.rest.common.TempoWorklogAttributeTypeIdentifier.StaticList)
            .Unnest(
                retrieveNestedCollection: attrDef => attrDef.StaticListValues ?? Array.Empty<jira.api.rest.common.TempoWorklogAttributeStaticListValue>(),
                resultSelector: (outer, inner) => inner
            )
            .ToDictionary(staticListItem => staticListItem.Value);
    }

    private async Task<(string, jira.api.rest.response.JiraIssueWorklogsWorklog)[]> RetrieveWorklogsForDeletion(JiraWorklog[] inputWorklogs)
    {
        if (_jiraClient == null)
            throw new ArgumentNullException("FATAL! Jira client not initialized");

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

        _feedback.RetrieveWorklogsForDeletionSetTarget(currentIssueWorklogsTasks.Count + 1);

        await MultiTask.WhenAll(
            tasks: currentIssueWorklogsTasks.Select(x => x.Value),
            reportProgress: (progress, _) => _feedback.RetrieveWorklogsForDeletionProcess(progress)
        );

        return currentIssueWorklogsTasks
            .Unnest(
                retrieveNestedCollection: x => x.Value.Result.Worklogs
                    .Where(worklog => worklog.Author.Name.Equals(_config.UserConfig.JiraUserName, StringComparison.OrdinalIgnoreCase))
                    .Where(worklog => worklog.Created.Value >= minInputWorklogDay && worklog.Created.Value < supInputWorklogDay),
                resultSelector: (outer, inner) => new ValueTuple<string, jira.api.rest.response.JiraIssueWorklogsWorklog>(outer.Key, inner)
            )
            .ToArray();
    }
}
