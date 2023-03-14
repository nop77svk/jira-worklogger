#pragma warning disable SA1313
namespace jwl.core;

public record JiraWorklog(
    JiraIssueKey IssueKey,
    DateTime Date,
    TimeSpan TimeSpent,
    string TempoWorklogType,
    string Comment
)
{ }
