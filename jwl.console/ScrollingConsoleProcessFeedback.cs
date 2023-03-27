namespace jwl.console;
using jwl.core;
using jwl.infra;
using ShellProgressBar;

public class ScrollingConsoleProcessFeedback
    : ICoreProcessFeedback, IDisposable
{
    public Action? FeedbackDelay { get; init; } = null;

    internal const string ProgressBarMsg = @"Filling Jira worklogs for you";
    private bool _isDisposed;

    public ScrollingConsoleProcessFeedback(int totalSteps)
    {
    }

    public void DeleteExistingWorklogsStart()
    {
        Console.Error.WriteLine(ProgressBarMsg);
        Console.Error.WriteLine(@"    Deleting existing worklogs");
    }

    public void DeleteExistingWorklogsSetTarget(int numberOfWorklogs)
    {
        Console.Error.WriteLine($"    Deleting {numberOfWorklogs} existing worklogs");
    }

    public void DeleteExistingWorklogsProcess(MultiTaskProgress progress)
    {
        Console.Error.WriteLine($"    Deleted {progress.Finished} worklogs, failed to delete {progress.ErredSoFar} worklogs thus far");
    }

    public void DeleteExistingWorklogsEnd()
    {
        Console.Error.WriteLine($"    Done deleting existing worklogs");
    }

    public void Dispose()
    {
        // note: Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FillJiraWithWorklogsStart()
    {
        Console.Error.WriteLine(ProgressBarMsg);
        Console.Error.WriteLine(@"    Filling Jira with your worklogs");
    }

    public void FillJiraWithWorklogsSetTarget(int numberOfWorklogs)
    {
        Console.Error.WriteLine($"    Filling Jira with {numberOfWorklogs} worklogs");
    }

    public void FillJiraWithWorklogsProcess(MultiTaskProgress progress)
    {
        Console.Error.WriteLine($"    Added {progress.Finished} worklogs, failed to add {progress.ErredSoFar} worklogs thus far");
    }

    public void FillJiraWithWorklogsEnd()
    {
        Console.Error.WriteLine(@"    Done filling Jira with your worklogs");
    }

    public void OverallProcessEnd()
    {
        Console.Error.WriteLine(ProgressBarMsg + " :: DONE");
    }

    public void OverallProcessStart()
    {
        Console.Error.WriteLine(ProgressBarMsg + " :: STARTING");
    }

    public void PreloadAvailableWorklogTypesEnd()
    {
    }

    public void PreloadAvailableWorklogTypesStart()
    {
        Console.Error.WriteLine(ProgressBarMsg + " :: Preloading available worklog types from server");
    }

    public void PreloadUserInfoStart(string userName)
    {
        Console.Error.WriteLine(ProgressBarMsg + $" :: Preloading user \"{userName}\" info from server");
    }

    public void PreloadUserInfoEnd()
    {
    }

    public void ReadCsvInputStart()
    {
        Console.Error.WriteLine(ProgressBarMsg);
        Console.Error.WriteLine(@"    Reading your input files");
    }

    public void ReadCsvInputSetTarget(int numberOfInputFiles)
    {
        Console.Error.WriteLine($"    Reading {numberOfInputFiles} input files");
    }

    public void ReadCsvInputProcess(MultiTaskProgress progress)
    {
        Console.Error.WriteLine($"    Read {progress.Finished} input files, failed to do so on {progress.ErredSoFar} files thus far");
    }

    public void ReadCsvInputEnd()
    {
        Console.Error.WriteLine(@"    Done reading your input files");
    }

    public void RetrieveWorklogsForDeletionStart()
    {
        Console.Error.WriteLine(ProgressBarMsg);
        Console.Error.WriteLine(@"    Retrieving list of worklogs to be deleted");
    }

    public void RetrieveWorklogsForDeletionSetTarget(int count)
    {
        Console.Error.WriteLine($"    Retrieving list of worklogs ({count} Jira issues) to be deleted");
    }

    public void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress)
    {
        Console.Error.WriteLine($"    Retrieved list of worklogs from {progress.Finished} issues, error from {progress.ErredSoFar} issues");
    }

    public void RetrieveWorklogsForDeletionEnd()
    {
        Console.Error.WriteLine(@"    Done retrieving list of worklogs to be deleted");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }
}
