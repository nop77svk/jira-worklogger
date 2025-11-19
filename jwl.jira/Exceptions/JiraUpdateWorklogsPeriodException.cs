namespace jwl.Jira.Exceptions;

public class JiraUpdateWorklogsPeriodException : JiraIssueSpecificException
{
    public long WorklogId { get; }
    public DateTime FromMoment { get; }
    public DateTime ToMoment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public JiraUpdateWorklogsPeriodException(string issueKey, long worklogId, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds)
        : this(issueKey, worklogId, fromMoment, toMoment, timeSpentSeconds, (string?)null)
    {
    }

    public JiraUpdateWorklogsPeriodException(string issueKey, long worklogId, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? message)
        : base(issueKey, FormatMessage(worklogId, fromMoment, toMoment, timeSpentSeconds, message))
    {
        WorklogId = worklogId;
        FromMoment = fromMoment;
        ToMoment = toMoment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public JiraUpdateWorklogsPeriodException(string issueKey, long worklogId, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, Exception innerException)
        : this(issueKey, worklogId, fromMoment, toMoment, timeSpentSeconds, (string?)null, innerException)
    {
    }

    public JiraUpdateWorklogsPeriodException(string issueKey, long worklogId, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? message, Exception innerException)
        : base(issueKey, FormatMessage(worklogId, fromMoment, toMoment, timeSpentSeconds, message), innerException)
    {
        WorklogId = worklogId;
        FromMoment = fromMoment;
        ToMoment = toMoment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    private static string FormatMessage(long worklogId, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? optionalMessage)
        => DefaultMessageFormatter($"Error updating worklog(s) ID {worklogId} with {timeSpentSeconds} during {fromMoment}-{toMoment} on the issue", optionalMessage);
}
