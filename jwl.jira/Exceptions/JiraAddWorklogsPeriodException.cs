namespace jwl.jira.Exceptions;

using System;
using jwl.Jira.Exceptions;

public class JiraAddWorklogsPeriodException : JiraIssueSpecificException
{
    public DateTime FromMoment { get; }
    public DateTime ToMoment { get; }
    public int TimeSpentSeconds { get; }
    public string? Activity { get; init; }
    public string? Comment { get; init; }

    public JiraAddWorklogsPeriodException(string issueKey, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds)
        : this(issueKey, fromMoment, toMoment, timeSpentSeconds, (string?)null)
    {
    }

    public JiraAddWorklogsPeriodException(string issueKey, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? message)
        : base(issueKey, FormatMessage(fromMoment, toMoment, timeSpentSeconds, message))
    {
        FromMoment = fromMoment;
        ToMoment = toMoment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    public JiraAddWorklogsPeriodException(string issueKey, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, Exception innerException)
        : this(issueKey, fromMoment, toMoment, timeSpentSeconds, (string?)null, innerException)
    {
    }

    public JiraAddWorklogsPeriodException(string issueKey, DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? message, Exception innerException)
        : base(issueKey, FormatMessage(fromMoment, toMoment, timeSpentSeconds, message), innerException)
    {
        FromMoment = fromMoment;
        ToMoment = toMoment;
        TimeSpentSeconds = timeSpentSeconds;
    }

    private static string FormatMessage(DateTime fromMoment, DateTime toMoment, int timeSpentSeconds, string? optionalMessage)
        => DefaultMessageFormatter($"Error adding worklog with {timeSpentSeconds} during {fromMoment}-{toMoment} to the issue", optionalMessage);
}
