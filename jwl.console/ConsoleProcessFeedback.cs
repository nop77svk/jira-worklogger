namespace jwl.console;
using jwl.core;
using jwl.infra;
using ShellProgressBar;

public class ConsoleProcessFeedback
    : ICoreProcessFeedback, IDisposable
{
    public Action? FeedbackDelay { get; init; } = null;

    internal const string ProgressBarMsg = @"Filling Jira worklogs for you";
    private ProgressBar _overallProgress;
    private bool _isDisposed;
    private IProgressBar? _worklogsToBeDeletedProgress = null;
    private IProgressBar? _deleteExistingWorklogsProgress = null;
    private IProgressBar? _fillJiraWithWorklogsProgress = null;
    private IProgressBar? _readingInputFilesProgress = null;

    public ConsoleProcessFeedback(int totalSteps)
    {
        _overallProgress = new ProgressBar(totalSteps, ProgressBarMsg, new ProgressBarOptions()
        {
            ShowEstimatedDuration = true,
            CollapseWhenFinished = true,
            EnableTaskBarProgress = true,
            ProgressBarOnBottom = true,
            ProgressCharacter = '─'
        });
    }

    public void DeleteExistingWorklogsStart()
    {
        _overallProgress.Tick(ProgressBarMsg);
        _deleteExistingWorklogsProgress = _overallProgress.Spawn(0, "Deleting existing worklogs");
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
        _overallProgress.Tick(ProgressBarMsg);
        _fillJiraWithWorklogsProgress = _overallProgress.Spawn(0, "Filling Jira with your worklogs");
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

    public void OverallProcessEnd()
    {
        _overallProgress.Tick(ProgressBarMsg + " :: DONE");
        FeedbackDelay?.Invoke();
    }

    public void OverallProcessStart()
    {
        FeedbackDelay?.Invoke();
    }

    public void PreloadAvailableWorklogTypesEnd()
    {
    }

    public void PreloadAvailableWorklogTypesStart()
    {
        _overallProgress.Tick(ProgressBarMsg + " :: Preloading available worklog types from server");
        FeedbackDelay?.Invoke();
    }

    public void PreloadUserInfoStart(string userName)
    {
        _overallProgress.Tick(ProgressBarMsg + $" :: Preloading user \"{userName}\" info from server");
        FeedbackDelay?.Invoke();
    }

    public void PreloadUserInfoEnd()
    {
    }

    public void ReadCsvInputStart()
    {
        _overallProgress.Tick(ProgressBarMsg);
        _readingInputFilesProgress = _overallProgress.Spawn(0, @"Reading your input files");
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
        _overallProgress.Tick(ProgressBarMsg);
        _worklogsToBeDeletedProgress = _overallProgress.Spawn(0, "Retrieving list of worklogs to be deleted");
        FeedbackDelay?.Invoke();
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
                _worklogsToBeDeletedProgress?.Dispose();
                _overallProgress.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }
}
