namespace jwl.Jira.Exceptions;

[Serializable]
internal class JiraDeleteWorklogByIssueIdException
    : JiraClientException
{
    public long IssueId { get; }
    public long WorklogId { get; }

    public JiraDeleteWorklogByIssueIdException(long issueId, long worklogId)
        : this(issueId, worklogId, (string?)null)
    {
    }

    public JiraDeleteWorklogByIssueIdException(long issueId, long worklogId, string? message)
        : base(FormatMessage(issueId, worklogId, message))
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    public JiraDeleteWorklogByIssueIdException(long issueId, long worklogId, Exception innerException)
        : this(issueId, worklogId, (string?)null, innerException)
    {
    }

    public JiraDeleteWorklogByIssueIdException(long issueId, long worklogId, string? message, Exception innerException)
        : base(FormatMessage(issueId, worklogId, message), innerException)
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    private static string FormatMessage(long issueId, long worklogId, string? optionalMessage)
        => DefaultMessageFormatter($"Error deleting worklog ID {worklogId} on issue ID {issueId}", optionalMessage);
}