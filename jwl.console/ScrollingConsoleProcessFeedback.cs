namespace Jwl.Console;

using System.Reflection;

using Jwl.Core;

public class ScrollingConsoleProcessFeedback
    : ICoreProcessFeedback
{
    private bool _isDisposed;
    private int _numberOfWorklogsToInsert = 0;
    private int _numberOfWorklogsToDelete = 0;

    public Action? FeedbackDelay { get; init; } = null;

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
        System.Console.Error.Write(@"Filling Jira with worklogs...");
    }

    public void FillJiraWithWorklogsSetTarget(int numberOfWorklogsToInsert, int numberOfWorklogsToDelete)
    {
        _numberOfWorklogsToInsert = numberOfWorklogsToInsert;
        _numberOfWorklogsToDelete = numberOfWorklogsToDelete;
        System.Console.Error.Write($"\rFilling Jira with worklogs (+{_numberOfWorklogsToInsert}/-{_numberOfWorklogsToDelete})...");
    }

    public void FillJiraWithWorklogsProcess(MultiTaskStats progress)
    {
        System.Console.Error.Write($"\rFilling Jira with worklogs (+{_numberOfWorklogsToInsert}/-{_numberOfWorklogsToDelete})... {ProgressPercentageAsString(progress)}");
    }

    public void FillJiraWithWorklogsEnd()
    {
        _numberOfWorklogsToInsert = 0;
        _numberOfWorklogsToDelete = 0;
        System.Console.Error.WriteLine();
    }

    public void NoExistingWorklogsToDelete()
    {
    }

    public void NoFilesOnInput()
    {
        System.Console.Error.WriteLine(@"Note: No files on input - no work to be done");
    }

    public void NoWorklogsToFill()
    {
        System.Console.Error.WriteLine(@"Note: Empty files on input - no work to be done");
    }

    public void OverallProcessStart()
    {
        Assembly exe = Assembly.GetExecutingAssembly();
        object productName = exe.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyTitleAttribute))
            .ConstructorArguments[0].Value
            ?? "<unknown product>";
        object cliVersion = exe.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            .ConstructorArguments[0].Value
            ?? "?.?.?";

        Assembly? core = Assembly.GetAssembly(typeof(JwlCoreProcess));
        object coreVersion = core?.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            .ConstructorArguments[0].Value
            ?? "?.?.?";
        object coreCopyright = core?.CustomAttributes
            .First(x => x.AttributeType == typeof(AssemblyCopyrightAttribute))
            .ConstructorArguments[0].Value
            ?? "copy rights undetermined";

        System.Console.Error.WriteLine($"{productName} {cliVersion} (core {coreVersion})");
        System.Console.Error.WriteLine($"by Peter H., {coreCopyright}");
        System.Console.Error.WriteLine(new string('-', System.Console.WindowWidth - 1));
    }

    public void OverallProcessEnd()
    {
        System.Console.Error.WriteLine(@"DONE");
    }

    public void PreloadAvailableWorklogTypesStart()
    {
        System.Console.Error.Write(@"Preloading available worklog types from server...");
    }

    public void PreloadAvailableWorklogTypesEnd()
    {
        System.Console.Error.WriteLine(@" OK");
    }

    public void PreloadUserInfoStart(string userName)
    {
        System.Console.Error.Write($"Preloading user \"{userName}\" info from server...");
    }

    public void PreloadUserInfoEnd()
    {
        System.Console.Error.WriteLine(@" OK");
    }

    public void ReadCsvInputStart()
    {
        System.Console.Error.Write(@"Reading input files...");
    }

    public void ReadCsvInputSetTarget(int numberOfInputFiles)
    {
        System.Console.Error.Write($"\rReading {numberOfInputFiles} input files...");
    }

    public void ReadCsvInputProcess(MultiTaskStats progress)
    {
        System.Console.Error.Write($"\rReading {progress.Total} input files... {ProgressPercentageAsString(progress)}");
    }

    public void ReadCsvInputEnd()
    {
        System.Console.Error.WriteLine();
    }

    public void RetrieveWorklogsForDeletionStart()
    {
        System.Console.Error.Write(@"Retrieving list of worklogs to be deleted...");
    }

    public void RetrieveWorklogsForDeletionSetTarget(int count)
    {
        System.Console.Error.Write($"\rRetrieving list of worklogs ({count} Jira issues) to be deleted...");
    }

    public void RetrieveWorklogsForDeletionProcess(MultiTaskStats progress)
    {
        throw new NotImplementedException(@"--- checkpoint ---");
    }

    public void RetrieveWorklogsForDeletionEnd()
    {
        System.Console.Error.WriteLine(" OK");
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
                // nothing as of now
            }

            _isDisposed = true;
        }
    }
}
