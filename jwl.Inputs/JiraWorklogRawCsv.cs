#pragma warning disable SA1313
namespace Jwl.Inputs;

public record JiraWorklogRawCsv
(
    string IssueKey,
    string Date,
    string TimeSpent,
    string WorkLogActivity,
    string WorkLogComment
)
{ }
