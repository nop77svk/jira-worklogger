namespace jwl.console;
using jwl.core;
using jwl.infra;

public class ScrollingConsoleProcessFeedback
    : ICoreProcessFeedback, IDisposable
{
    public Action? FeedbackDelay { get; init; } = null;

    private bool _isDisposed;

    public ScrollingConsoleProcessFeedback(int totalSteps)
    {
    }

    public void DeleteExistingWorklogsStart()
    {
        Console.Error.Write(@"Deleting existing worklogs...");
    }

    public void DeleteExistingWorklogsSetTarget(int numberOfWorklogs)
    {
        Console.Error.Write($"\rDeleting {numberOfWorklogs} existing worklogs...");
    }

    public void DeleteExistingWorklogsProcess(MultiTaskProgress progress)
    {
        Console.Error.Write($"\rDeleting {progress.Total} existing worklogs... {ProgressPercentageAsString(progress)}");
    }

    public void DeleteExistingWorklogsEnd()
    {
        Console.Error.WriteLine();
    }

    public void Dispose()
    {
        // note: Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FillJiraWithWorklogsStart()
    {
        Console.Error.Write(@"Filling Jira with worklogs...");
    }

    public void FillJiraWithWorklogsSetTarget(int numberOfWorklogs)
    {
        Console.Error.Write($"\rFilling Jira with {numberOfWorklogs} worklogs...");
    }

    public void FillJiraWithWorklogsProcess(MultiTaskProgress progress)
    {
        Console.Error.Write($"\rFilling Jira with {progress.Total} worklogs... {ProgressPercentageAsString(progress)}");
    }

    public void FillJiraWithWorklogsEnd()
    {
        Console.Error.WriteLine();
    }

    public void NoExistingWorklogsToDelete()
    {
    }

    public void NoFilesOnInput()
    {
        Console.Error.WriteLine(@"No files on input - no work to be done");
    }

    public void NoWorklogsToFill()
    {
        Console.Error.WriteLine(@"Empty files on input - no work to be done");
    }

    public void OverallProcessStart()
    {
        Console.Error.WriteLine(@"STARTING");
    }

    public void OverallProcessEnd()
    {
        Console.Error.WriteLine(@"DONE");
    }

    public void PreloadAvailableWorklogTypesStart()
    {
        Console.Error.Write(@"Preloading available worklog types from server...");
    }

    public void PreloadAvailableWorklogTypesEnd()
    {
        Console.Error.WriteLine(@" OK");
    }

    public void PreloadUserInfoStart(string userName)
    {
        Console.Error.Write($"Preloading user \"{userName}\" info from server...");
    }

    public void PreloadUserInfoEnd()
    {
        Console.Error.WriteLine(@" OK");
    }

    public void ReadCsvInputStart()
    {
        Console.Error.Write(@"Reading input files...");
    }

    public void ReadCsvInputSetTarget(int numberOfInputFiles)
    {
        Console.Error.Write($"\rReading {numberOfInputFiles} input files...");
    }

    public void ReadCsvInputProcess(MultiTaskProgress progress)
    {
        Console.Error.Write($"\rReading {progress.Total} input files... {ProgressPercentageAsString(progress)}");
    }

    public void ReadCsvInputEnd()
    {
        Console.Error.WriteLine();
    }

    public void RetrieveWorklogsForDeletionStart()
    {
        Console.Error.Write(@"Retrieving list of worklogs to be deleted...");
    }

    public void RetrieveWorklogsForDeletionSetTarget(int count)
    {
        Console.Error.Write($"\rRetrieving list of worklogs ({count} Jira issues) to be deleted...");
    }

    public void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress)
    {
        Console.Error.Write($"\rRetrieved list of worklogs ({progress.Total} Jira issues) to be deleted... {ProgressPercentageAsString(progress)}");
    }

    public void RetrieveWorklogsForDeletionEnd()
    {
        Console.Error.WriteLine();
    }

    protected static string ProgressPercentageAsString(MultiTaskProgress progress)
    {
        string result;

        if (progress.Total <= 0)
        {
            result = @"done";
        }
        else
        {
            result = progress.SucceededPct.ToString("P");

            if (progress.ErredSoFar > 0)
            {
                result += " OK, " + progress.ErredSoFarPct.ToString("P") + " failed";
            }
        }

        return result;
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
