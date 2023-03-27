namespace jwl.inputs;
using jwl.jira;

public static class WorklogReaderFactory
{
    public static IWorklogReader GetCsvReaderFromStdin(CsvFormatConfig csvFormatConfig)
    {
        return new WorklogCsvReader(Console.In, csvFormatConfig);
    }

    public static IWorklogReader GetReaderFromFilePath(string inputPath, CsvFormatConfig csvFormatConfig) // 2do! rework to a delegate instead to loosen the dependency
    {
        IWorklogReader result;

        if (inputPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Csv, new FileStream(inputPath, FileMode.Open, FileAccess.Read), csvFormatConfig);
        }
        else if (inputPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Xlsx, new FileStream(inputPath, FileMode.Open, FileAccess.Read), csvFormatConfig);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Unrecognized file extension \"{inputPath}\"");
        }

        return result;
    }

    public static IWorklogReader GetReaderFromFileStream(WorklogReaderFileFormat fileFormat, Stream input, CsvFormatConfig csvFormatConfig)
    {
        return fileFormat switch
        {
            WorklogReaderFileFormat.Csv => new WorklogCsvReader(new StreamReader(input), csvFormatConfig),
            _ => throw new NotImplementedException() // 2do
        };
    }
}
