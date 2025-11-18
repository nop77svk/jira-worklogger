namespace jwl.Inputs;

public static class WorklogReaderFactory
{
    public static IWorklogReader GetReaderFromFilePath(string inputPath, WorklogReaderAggregatedConfig readerConfig) // 2do! rework to a delegate instead to loosen the dependency
    {
        IWorklogReader result;

        if (inputPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Csv, new FileStream(inputPath, FileMode.Open, FileAccess.Read), readerConfig);
        }
        else if (inputPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result = GetReaderFromFileStream(WorklogReaderFileFormat.Xlsx, new FileStream(inputPath, FileMode.Open, FileAccess.Read), readerConfig);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Unrecognized file extension \"{inputPath}\"");
        }

        return result;
    }

    public static IWorklogReader GetReaderFromFileStream(WorklogReaderFileFormat fileFormat, Stream input, WorklogReaderAggregatedConfig csvFormatConfig)
    {
        return fileFormat switch
        {
            WorklogReaderFileFormat.Csv => new WorklogCsvReader(new StreamReader(input), csvFormatConfig),
            WorklogReaderFileFormat.Xlsx => throw new NotImplementedException("Using XLSX inputs not yet implemented"),
            _ => throw new ArgumentOutOfRangeException(nameof(fileFormat), fileFormat, $"Unrecognized file format")
        };
    }
}
