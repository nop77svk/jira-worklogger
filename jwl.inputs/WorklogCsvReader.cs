namespace jwl.inputs;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using jwl.Infra;
using jwl.Jira;

public class WorklogCsvReader : IWorklogReader
{
    public bool ErrorOnEmptyRow { get; init; } = true;

    private CsvReader _csvReader;
    private WorklogReaderAggregatedConfig _readerConfig;

    public WorklogCsvReader(TextReader inputFile, WorklogReaderAggregatedConfig readerConfig)
    {
        _readerConfig = readerConfig;
        CsvConfiguration config = new (CultureInfo.InvariantCulture)
        {
            Delimiter = readerConfig.CsvFormatConfig?.FieldDelimiter ?? ","
        };

        _csvReader = new CsvReader(inputFile, config);
    }

    public IEnumerable<InputWorkLog> Read(Action<InputWorkLog>? postProcessResult = null)
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
                    throw new InvalidDataException("Empty row on input");
                else
                    continue;
            }

            InputWorkLog result;

            try
            {
                JiraIssueKey worklogIssueKey = new JiraIssueKey(row.IssueKey);

                if (!DateTime.TryParseExact(row.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime worklogDate))
                    throw new FormatException($"Invalid date/datetime value \"{row.Date}\"");

                TimeSpan worklogTimeSpent = HumanReadableTimeSpan.Parse(row.TimeSpent, timespanTimeFormats);

                result = new InputWorkLog()
                {
                    IssueKey = worklogIssueKey,
                    Date = worklogDate,
                    TimeSpent = worklogTimeSpent,
                    WorkLogActivity = row.WorkLogActivity,
                    WorkLogComment = row.WorkLogComment
                };

                postProcessResult?.Invoke(result);
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
        _csvReader.Dispose();
    }
}
