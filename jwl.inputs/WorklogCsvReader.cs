namespace jwl.inputs;
using System.Globalization;
using CsvHelper;
using jwl.jira;
using jwl.infra;

public class WorklogCsvReader : IWorklogReader
{
    public bool ErrorOnEmptyRow { get; init; } = true;

    private CsvReader _csvReader;

    public WorklogCsvReader(TextReader inputFile)
    {
        _csvReader = new CsvReader(inputFile, CultureInfo.InvariantCulture);
    }

    public IEnumerable<JiraWorklog> AsEnumerable()
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

            JiraWorklog result;

            try
            {
                JiraIssueKey worklogIssueKey = new JiraIssueKey(row.IssueKey);

                if (!DateTime.TryParseExact(row.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime worklogDate))
                    throw new FormatException($"Invalid date/datetime value \"{row.Date}\"");

                TimeSpan worklogTimeSpent = HumanReadableTimeSpan.Parse(row.TimeSpent, timespanTimeFormats);

                result = new JiraWorklog()
                {
                    IssueKey = worklogIssueKey,
                    Date = worklogDate,
                    TimeSpent = worklogTimeSpent,
                    TempWorklogType = row.TempoWorklogType,
                    Comment = row.Comment
                };
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

    private void ParseHeader()
    {
        // 2do!
    }
}
