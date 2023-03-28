namespace jwl.console;
using jwl.core;
using jwl.infra;
using ShellProgressBar;

public class ConsoleProcessFeedback
    : ICoreProcessFeedback, IDisposable
{
    public Action? FeedbackDelay { get; init; } = null;

    private readonly ProgressBarOptions _overallOptions = new ProgressBarOptions()
    {
        ShowEstimatedDuration = false,
        CollapseWhenFinished = true,
        EnableTaskBarProgress = true,
        ProgressBarOnBottom = true,
        DenseProgressBar = false,
        ProgressCharacter = '─',
        DisplayTimeInRealTime = false,
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = Console.ForegroundColor,
        ForegroundColorError = ConsoleColor.Red
    };

    private bool _isDisposed;
    private IProgressBar? _worklogsToBeDeletedProgress = null;
    private IProgressBar? _deleteExistingWorklogsProgress = null;
    private IProgressBar? _fillJiraWithWorklogsProgress = null;
    private IProgressBar? _readingInputFilesProgress = null;

    public ConsoleProcessFeedback(int totalSteps)
    {
    }

    public void DeleteExistingWorklogsStart()
    {
        _deleteExistingWorklogsProgress = GenericStepStart(@"Deleting existing worklogs");
        FeedbackDelay?.Invoke();
    }

    public void DeleteExistingWorklogsSetTarget(int numberOfWorklogs)
    {
        if (_deleteExistingWorklogsProgress != null)
            _deleteExistingWorklogsProgress.MaxTicks = numberOfWorklogs;

        FeedbackDelay?.Invoke();
    }

    public void DeleteExistingWorklogsProcess(MultiTaskProgress progress)
    {
        GenericMultiTaskProgressProcessFeedback(_deleteExistingWorklogsProgress, progress);
        FeedbackDelay?.Invoke();
    }

    public void DeleteExistingWorklogsEnd()
    {
        _deleteExistingWorklogsProgress?.WriteErrorLine(string.Empty);
        _deleteExistingWorklogsProgress?.Dispose();
        _deleteExistingWorklogsProgress = null;
        FeedbackDelay?.Invoke();
    }

    public void Dispose()
    {
        // note: Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FillJiraWithWorklogsStart()
    {
        _fillJiraWithWorklogsProgress = GenericStepStart(@"Filling your worklogs to Jira");
        FeedbackDelay?.Invoke();
    }

    public void FillJiraWithWorklogsSetTarget(int numberOfWorklogs)
    {
        if (_fillJiraWithWorklogsProgress != null)
            _fillJiraWithWorklogsProgress.MaxTicks = numberOfWorklogs;

        FeedbackDelay?.Invoke();
    }

    public void FillJiraWithWorklogsProcess(MultiTaskProgress progress)
    {
        GenericMultiTaskProgressProcessFeedback(_fillJiraWithWorklogsProgress, progress);
        FeedbackDelay?.Invoke();
    }

    public void FillJiraWithWorklogsEnd()
    {
        _fillJiraWithWorklogsProgress?.Dispose();
        _fillJiraWithWorklogsProgress = null;
        FeedbackDelay?.Invoke();
    }

    public void NoExistingWorklogsToDelete()
    {
    }

    public void NoFilesOnInput()
    {
    }

    public void NoWorklogsToFill()
    {
    }

    public void OverallProcessEnd()
    {
        Console.Error.WriteLine(@"DONE");
        FeedbackDelay?.Invoke();
    }

    public void OverallProcessStart()
    {
        Console.Error.WriteLine(@"STARTING");
        FeedbackDelay?.Invoke();
    }

    public void PreloadAvailableWorklogTypesEnd()
    {
    }

    public void PreloadAvailableWorklogTypesStart()
    {
        Console.Error.WriteLine(@"Preloading available worklog types from server");
        FeedbackDelay?.Invoke();
    }

    public void PreloadUserInfoStart(string userName)
    {
        Console.Error.WriteLine($"Preloading user \"{userName}\" info from server");
        FeedbackDelay?.Invoke();
    }

    public void PreloadUserInfoEnd()
    {
    }

    public void ReadCsvInputStart()
    {
        _readingInputFilesProgress = GenericStepStart(@"Reading input files");
        FeedbackDelay?.Invoke();
    }

    public void ReadCsvInputSetTarget(int numberOfInputFiles)
    {
        if (_readingInputFilesProgress != null)
            _readingInputFilesProgress.MaxTicks = numberOfInputFiles;

        FeedbackDelay?.Invoke();
    }

    public void ReadCsvInputProcess(MultiTaskProgress progress)
    {
        GenericMultiTaskProgressProcessFeedback(_readingInputFilesProgress, progress);
        FeedbackDelay?.Invoke();
    }

    public void ReadCsvInputEnd()
    {
    }

    public void RetrieveWorklogsForDeletionEnd()
    {
        _worklogsToBeDeletedProgress?.Dispose();
        _worklogsToBeDeletedProgress = null;
        FeedbackDelay?.Invoke();
    }

    public void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress)
    {
        GenericMultiTaskProgressProcessFeedback(_worklogsToBeDeletedProgress, progress);
        FeedbackDelay?.Invoke();
    }

    public void RetrieveWorklogsForDeletionSetTarget(int count)
    {
        if (_worklogsToBeDeletedProgress != null)
            _worklogsToBeDeletedProgress.MaxTicks = count;
        FeedbackDelay?.Invoke();
    }

    public void RetrieveWorklogsForDeletionStart()
    {
        _worklogsToBeDeletedProgress = GenericStepStart(@"Retrieving list of worklogs to be deleted");
        FeedbackDelay?.Invoke();
    }

    protected ProgressBar GenericStepStart(string message, int maxProgressTicks = 0)
    {
        return new ProgressBar(maxProgressTicks, message, _overallOptions);
    }

    protected void GenericMultiTaskProgressProcessFeedback(IProgressBar? bar, MultiTaskProgress progress)
    {
        bar?.Tick(progress.DoneSoFar, progress.ErredSoFar > 0 ? $"({progress.ErredSoFar} errors thus far)" : null);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _deleteExistingWorklogsProgress?.Dispose();
                _fillJiraWithWorklogsProgress?.Dispose();
                _readingInputFilesProgress?.Dispose();
                _worklogsToBeDeletedProgress?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }
}
