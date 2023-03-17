#pragma warning disable SA1313
namespace jwl.core;

public record JiraWorklog
{
    public JiraIssueKey IssueKey;
    public DateTime Date;
    public TimeSpan TimeSpent;
    public string TempWorklogType;
    public string Comment;
}
