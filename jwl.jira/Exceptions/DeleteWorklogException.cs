namespace Jwl.Jira.Exceptions;

internal class DeleteWorklogException
    : JiraIssueSpecificException
{
    public long IssueId { get; }
    public long WorklogId { get; }

    public DeleteWorklogException(long issueId, long worklogId)
        : base($"ID {issueId}", $"Error deleting worklog ID {worklogId} on issue ID {issueId}")
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    public DeleteWorklogException(long issueId, long worklogId, Exception innerException)
        : base($"ID {issueId}", $"Error deleting worklog ID {worklogId} on issue ID {issueId}", innerException)
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }
}
