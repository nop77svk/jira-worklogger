namespace jwl.Jira.Exceptions;

using System;

public class GetIssueWorkLogsException
    : JiraIssueSpecificException
{
    public DateTime DateFrom { get; }
    public DateTime DateTo { get; }

    public GetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo)
        : base(issueKey, $"Error retrieving worklogs for {issueKey} and timestamp range from {dateFrom} to {dateTo}")
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    public GetIssueWorkLogsException(string issueKey, DateTime dateFrom, DateTime dateTo, Exception innerException)
        : base(issueKey, $"Error retrieving worklogs for issue \"{issueKey}\" and period from {dateFrom} to {dateTo}", innerException)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
    }
}
