namespace jwl.Jira.Exceptions;

public class JiraDeleteWorklogException
    : JiraIssueSpecificException
{
    public long IssueId { get; }
    public long WorklogId { get; }

    public JiraDeleteWorklogException(long issueId, long worklogId)
        : this(issueId, worklogId, (string?)null)
    {
    }

    public JiraDeleteWorklogException(long issueId, long worklogId, string? message)
        : base($"ID {issueId}", FormatMessage(issueId, worklogId, message))
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    public JiraDeleteWorklogException(long issueId, long worklogId, Exception innerException)
        : this(issueId, worklogId, (string?)null, innerException)
    {
    }

    public JiraDeleteWorklogException(long issueId, long worklogId, string? message, Exception innerException)
        : base($"ID {issueId}", FormatMessage(issueId, worklogId, message), innerException)
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    private static string FormatMessage(long issueId, long worklogId, string? optionalMessage)
        => DefaultMessageFormatter($"Error deleting worklog ID {worklogId} on issue ID {issueId}", optionalMessage);
}
