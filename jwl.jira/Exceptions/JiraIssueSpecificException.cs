namespace jwl.Jira.Exceptions;

public class JiraIssueSpecificException
    : JiraClientException
{
    public string? IssueKey { get; }

    public JiraIssueSpecificException(string issueKey)
        : base($"Error on Jira issue {issueKey}")
    {
        IssueKey = issueKey;
    }

    public JiraIssueSpecificException(string issueKey, string message)
        : base(message)
    {
        IssueKey = issueKey;
    }

    public JiraIssueSpecificException(string issueKey, Exception innerException)
        : base($"Error on Jira issue {issueKey}", innerException)
    {
        IssueKey = issueKey;
    }

    public JiraIssueSpecificException(string issueKey, string message, Exception innerException)
        : base(message, innerException)
    {
        IssueKey = issueKey;
    }
}
