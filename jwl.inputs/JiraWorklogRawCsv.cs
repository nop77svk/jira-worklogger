#pragma warning disable SA1313
namespace jwl.inputs;

public record JiraWorklogRawCsv(
    string IssueKey,
    string Date,
    string TimeSpent,
    string WorklogType,
    string Comment
)
{ }
