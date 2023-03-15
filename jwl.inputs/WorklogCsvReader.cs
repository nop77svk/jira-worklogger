namespace jwl.inputs;
using System.Globalization;
using CsvHelper;
using jwl.core;

public class WorklogCsvReader : IWorklogReader
{
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

        _csvReader.Read();
        _csvReader.ReadHeader();

        int currentRowNo = 1;
        while (_csvReader.Read())
        {
            currentRowNo++;
            JiraWorklog result;

            try
            {
                JiraWorklogRawCsv? row = _csvReader.GetRecord<JiraWorklogRawCsv>();
                if (row == null)
                    throw new FormatException("Empty row on input");

                JiraIssueKey worklogIssueKey = new JiraIssueKey(row.IssueKey);

                if (!DateTime.TryParseExact(row.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime worklogDate))
                    throw new FormatException($"Invalid date/datetime value \"{row.Date}\"");

                TimeSpan worklogTimeSpent = LiberalParseTimeSpan(row.TimeSpent, timespanTimeFormats);

                result = new JiraWorklog(worklogIssueKey, worklogDate, worklogTimeSpent, row.TempoWorklogType, row.Comment);
            }
            catch (Exception e)
            {
                throw new InputRowException(currentRowNo, e);
            }

            yield return result;
        }
    }

    public void Dispose()
    {
        _csvReader.Dispose();
    }

    private TimeSpan LiberalParseTimeSpan(string timeSpanStr, string[] timespanTimeFormats)
    {
        TimeSpan result;

        if (!TimeSpan.TryParseExact(timeSpanStr.Replace(" ", null), timespanTimeFormats, CultureInfo.InvariantCulture, out result))
        {
            double timeSpanInNumberOfHours;
            NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            if (!double.TryParse(timeSpanStr, numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
            {
                if (!double.TryParse(timeSpanStr.Replace('.', ','), numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
                {
                    if (!double.TryParse(timeSpanStr.Replace(',', '.'), numberStyles, CultureInfo.InvariantCulture, out timeSpanInNumberOfHours))
                    {
                        throw new FormatException($"Invalid timespan value \"{timeSpanStr}\"");
                    }
                }
            }

            result = TimeSpan.FromHours(timeSpanInNumberOfHours);
        }

        return result;
    }

    private void ParseHeader()
    {
        // 2do!
    }
}