namespace jwl.inputs;
using jwl.core;

public static class WorklogReaderFactory
{
    public static IWorklogReader GetCsvReaderFromStdin()
    {
        return new WorklogCsvReader(Console.In);
    }

    public static IWorklogReader GetReaderFromFilePath(string inputPath)
    {
        IWorklogReader result;

        if (inputPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Csv, new FileStream(inputPath, FileMode.Open, FileAccess.Read));
        }
        else if (inputPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Xlsx, new FileStream(inputPath, FileMode.Open, FileAccess.Read));
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Unrecognized file extension \"{inputPath}\"");
        }

        return result;
    }

    public static IWorklogReader GetReaderFromFileStream(WorklogReaderFileFormat fileFormat, Stream input)
    {
        return fileFormat switch
        {
            WorklogReaderFileFormat.Csv => new WorklogCsvReader(new StreamReader(input)),
            _ => throw new NotImplementedException() // 2do
        };
    }
}
