namespace jwl.Jira.Exceptions;

public class JiraDeleteWorklogException
    : JiraIssueSpecificException
{
    public long IssueId { get; }
    public long WorklogId { get; }

    public JiraDeleteWorklogException(long issueId, long worklogId)
        : base($"ID {issueId}", $"Error deleting worklog ID {worklogId} on issue ID {issueId}")
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }

    public JiraDeleteWorklogException(long issueId, long worklogId, Exception innerException)
        : base($"ID {issueId}", $"Error deleting worklog ID {worklogId} on issue ID {issueId}", innerException)
    {
        IssueId = issueId;
        WorklogId = worklogId;
    }
}
