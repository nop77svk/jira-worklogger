namespace jwl.Inputs;

using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using jwl.Infra;

public class WorklogCsvReader : IWorklogReader
{
    private readonly CsvReader _csvReader;
    private readonly WorklogReaderAggregatedConfig _readerConfig;
    public bool ErrorOnEmptyRow { get; init; } = true;

    public WorklogCsvReader(TextReader inputFile, WorklogReaderAggregatedConfig readerConfig)
    {
        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            Delimiter = readerConfig.CsvFormatConfig?.FieldDelimiter ?? ","
        };

        _csvReader = new CsvReader(inputFile, config);
    }

    public IEnumerable<InputWorkLog> Read(Action<InputWorkLog>? validateResult = null)
    {
        string[] dateFormats =
        {
            @"yyyy-MM-dd",
            @"yyyy-MM-dd hh:mm:ss",
            @"yyyy/MM/dd",
            @"yyyy/MM/dd hh:mm:ss",
            @"dd.MM.YYYY",
            @"dd.MM.YYYY hh:mm:ss"
        };

        string[] timespanTimeFormats =
        {
            @"hh\:mm\:ss",
            @"hh\:mm",
            @"mm",
            @"hh'h'mm",
            @"hh'h'mm'm'"
        };

        foreach (JiraWorklogRawCsv row in _csvReader.GetRecords<JiraWorklogRawCsv>())
        {
            if (row == null)
            {
                if (ErrorOnEmptyRow)
                {
                    throw new InvalidDataException("Empty row on input");
                }

                continue;
            }

            InputWorkLog result;

            try
            {
                JiraIssueKey worklogIssueKey = new JiraIssueKey(row.IssueKey);

                if (!DateTime.TryParseExact(row.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime worklogDate))
                {
                    throw new FormatException($"Invalid date/datetime value \"{row.Date}\"");
                }

                TimeSpan worklogTimeSpent = HumanReadableTimeSpan.Parse(row.TimeSpent, timespanTimeFormats);

                result = new InputWorkLog()
                {
                    IssueKey = worklogIssueKey,
                    Date = worklogDate,
                    TimeSpent = worklogTimeSpent,
                    WorkLogActivity = row.WorkLogActivity,
                    WorkLogComment = row.WorkLogComment
                };

                validateResult?.Invoke(result);
            }
            catch (Exception e)
            {
                throw new InputRowException(_csvReader.Parser.RawRow, e);
            }

            yield return result;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _csvReader.Dispose();
            }

            _disposedValue = true;
        }
    }
}