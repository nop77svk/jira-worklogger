namespace jwl.inputs;
using System.Globalization;
using CsvHelper;
using jwl.core;

public class CsvReader : IDisposable
{
    private StreamReader _streamReader;
    private CsvHelper.CsvConfiguration _csvConfig;
    private CsvHelper.CsvReader _csvReader;

    public CsvReader(Stream inputFile)
    {
        _streamReader = new StreamReader(inputFile, detectEncodingFromByteOrderMarks: true);

        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        };

        _csvReader = new CsvReader(_streamReader, _csvConfig);
    }

    public IEnumerable<JiraWorklog> AsEnumerable()
    {
        string[] dateFormats = {
            @"yyyy-MM-dd",
            @"yyyy-MM-dd hh:mm:ss",
            @"yyyy/MM/dd",
            @"yyyy/MM/dd hh:mm:ss",
            @"dd.MM.YYYY",
            @"dd.MM.YYYY hh:mm:ss"
        };

        string[] timespanTimeFormats = {
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
            JiraWorklogRawCsv row = _csvReader.GetRecord<JiraWorklogRawCsv>();

            try
            {
                JiraIssueKey worklogIssueKey = new JiraIssueKey(row.IssueKey);

                if (!DateTime.TryParseExact(row.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime worklogDate))
                    throw new FormatException($"Invalid date/datetime value \"{row.Date}\"");

                TimeSpan worklogTimeSpent = LiberalParseTimeSpan(row.TimeSpent, timespanTimeFormats);

                yield return new JiraWorklog(worklogIssueKey, worklogDate, worklogTimeSpent, row.WorklogType, row.Comment);
            }
            catch (Exception e)
            {
                throw new InputRowException(currentRowNo);
            }
        }
    }

    public void Dispose()
    {
        _csvReader.Dispose();
        _streamReader.Dispose();
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
                        throw new FormatException($"Invalid time spent value \"{row.TimeSpent}\"");
                    }
                }
            }
            
            result = TimeSpan.FromHours(timeSpanInNumberOfHours);
            result.Seconds = 0;
        }

        return result;
    }

    private void ParseHeader()
    {
        
    }
}
