namespace jwl.Jira.Exceptions;

public abstract class JiraIssueSpecificException
    : JiraClientException
{
    public string IssueKey { get; }

    protected JiraIssueSpecificException(string issueKey)
        : base(FormatMessage(issueKey, null))
    {
        IssueKey = issueKey;
    }

    protected JiraIssueSpecificException(string issueKey, string message)
        : base(FormatMessage(issueKey, message))
    {
        IssueKey = issueKey;
    }

    protected JiraIssueSpecificException(string issueKey, Exception innerException)
        : base(FormatMessage(issueKey, null), innerException)
    {
        IssueKey = issueKey;
    }

    protected JiraIssueSpecificException(string issueKey, string message, Exception innerException)
        : base(FormatMessage(issueKey, message), innerException)
    {
        IssueKey = issueKey;
    }

    private static string FormatMessage(string issueKey, string? optionalMessage)
        => DefaultMessageFormatter($"Error on Jira issue {issueKey}", optionalMessage);
}
