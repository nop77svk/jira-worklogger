namespace jwl.console;
using jwl.core;
using jwl.infra;
using System.Reflection;

public class ScrollingConsoleProcessFeedback
    : ICoreProcessFeedback, IDisposable
{
    public Action? FeedbackDelay { get; init; } = null;

    private bool _isDisposed;
    private int _numberOfWorklogsToInsert = 0;
    private int _numberOfWorklogsToDelete = 0;

    public ScrollingConsoleProcessFeedback(int totalSteps)
    {
    }

    public void Dispose()
    {
        // note: Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FillJiraWithWorklogsStart()
    {
        _numberOfWorklogsToInsert = 0;
        _numberOfWorklogsToDelete = 0;
        Console.Error.Write(@"Filling Jira with worklogs...");
    }

    public void FillJiraWithWorklogsSetTarget(int numberOfWorklogsToInsert, int numberOfWorklogsToDelete)
    {
        _numberOfWorklogsToInsert = numberOfWorklogsToInsert;
        _numberOfWorklogsToDelete = numberOfWorklogsToDelete;
        Console.Error.Write($"\rFilling Jira with worklogs (+{_numberOfWorklogsToInsert}/-{_numberOfWorklogsToDelete})...");
    }

    public void FillJiraWithWorklogsProcess(MultiTaskStats progress)
    {
        Console.Error.Write($"\rFilling Jira with worklogs (+{_numberOfWorklogsToInsert}/-{_numberOfWorklogsToDelete})... {ProgressPercentageAsString(progress)}");
    }

    public void FillJiraWithWorklogsEnd()
    {
        _numberOfWorklogsToInsert = 0;
        _numberOfWorklogsToDelete = 0;
        Console.Error.WriteLine();
    }

    public void NoExistingWorklogsToDelete()
    {
    }

    public void NoFilesOnInput()
    {
        Console.Error.WriteLine(@"Note: No files on input - no work to be done");
    }

    public void NoWorklogsToFill()
    {
        Console.Error.WriteLine(@"Note: Empty files on input - no work to be done");
    }

    public void OverallProcessStart()
    {
        Assembly exe = Assembly.GetExecutingAssembly();
        object productName = exe.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyTitleAttribute))
            .ConstructorArguments.First().Value
            ?? "<unknown product>";
        object cliVersion = exe.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            .ConstructorArguments.First().Value
            ?? "?.?.?";

        Assembly? core = Assembly.GetAssembly(typeof(JwlCoreProcess));
        object coreVersion = core?.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            .ConstructorArguments.First().Value
            ?? "?.?.?";
        object coreCopyright = core?.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyCopyrightAttribute))
            .ConstructorArguments.First().Value
            ?? "copy rights undetermined";

        Console.Error.WriteLine($"{productName} {cliVersion} (core {coreVersion})");
        Console.Error.WriteLine($"by Peter H., {coreCopyright}");
        Console.Error.WriteLine(new string('-', Console.WindowWidth - 1));
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

    public void ReadCsvInputProcess(MultiTaskStats progress)
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

    public void RetrieveWorklogsForDeletionProcess(MultiTaskStats progress)
    {
        throw new NotImplementedException(@"--- checkpoint ---");
    }

    public void RetrieveWorklogsForDeletionEnd()
    {
        Console.Error.WriteLine(" OK");
    }

    protected static string ProgressPercentageAsString(MultiTaskStats progress)
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
