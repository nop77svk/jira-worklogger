namespace jwl.Jira.Exceptions;

using System;

public class JiraGetIssueWorkLogsException
    : JiraIssueSpecificException
{
    public DateTime DateFrom { get; }
    public DateTime DateTo { get; }

    public JiraGetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo)
        : this(issueKey, dateFrom, dateTo, (string?)null)
    {
    }

    public JiraGetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo, string? message)
        : base(issueKey, FormatMessage(dateFrom, dateTo, message))
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    public JiraGetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo, Exception innerException)
        : this(issueKey, dateFrom, dateTo, (string?)null, innerException)
    {
    }

    public JiraGetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo, string? message, Exception innerException)
        : base(issueKey, FormatMessage(dateFrom, dateTo, message), innerException)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    private static string FormatMessage(DateTime dateFrom, DateTime dateTo, string? optionalMessage)
        => DefaultMessageFormatter($"Error retrieving issue worklogs from {dateFrom} to {dateTo}", optionalMessage);
}