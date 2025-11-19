namespace jwl.Jira.Exceptions;

public class JiraUpdateWorklogException
    : JiraIssueSpecificException
{
    public long WorklogId { get; }
    public DateTime Moment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public JiraUpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds)
        : this(issueKey, worklogId, moment, timeSpentSeconds, (string?)null)
    {
    }

    public JiraUpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds, string? message)
        : base(issueKey, FormatMessage(worklogId, moment, timeSpentSeconds, message))
    {
        WorklogId = worklogId;
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public JiraUpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds, Exception innerException)
        : this(issueKey, worklogId, moment, timeSpentSeconds, (string?)null, innerException)
    {
    }

    public JiraUpdateWorklogException(string issueKey, long worklogId, DateTime moment, int timeSpentSeconds, string? message, Exception innerException)
        : base(issueKey, FormatMessage(worklogId, moment, timeSpentSeconds, message), innerException)
    {
        WorklogId = worklogId;
        Moment = moment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    private static string FormatMessage(long worklogId, DateTime moment, int timeSpentSeconds, string? optionalMessage)
        => DefaultMessageFormatter($"Error updating worklog ID {worklogId} on {moment} with {timeSpentSeconds} seconds", optionalMessage);
}
