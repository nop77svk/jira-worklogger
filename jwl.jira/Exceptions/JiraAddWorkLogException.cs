namespace jwl.Jira.Exceptions;

public class JiraAddWorkLogException
    : JiraIssueSpecificException
{
    public DateTime Moment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public JiraAddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds)
        : this(issueKey, moment, timeSpentSeconds, (string?)null)
    {
    }

    public JiraAddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds, string? message)
        : base(issueKey, FormatMessage(moment, timeSpentSeconds, message))
    {
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public JiraAddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds, Exception innerException)
        : this(issueKey, moment, timeSpentSeconds, (string?)null, innerException)
    {
    }

    public JiraAddWorkLogException(string issueKey, DateTime moment, int timeSpentSeconds, string? message, Exception innerException)
        : base(issueKey, FormatMessage(moment, timeSpentSeconds, message), innerException)
    {
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    private static string FormatMessage(DateTime moment, int timeSpentSeconds, string? optionalMessage)
        => DefaultMessageFormatter($"Error adding worklog with {timeSpentSeconds} seconds spent on {moment} to the issue", optionalMessage);
}
