#pragma warning disable SA1313
namespace jwl.inputs;

public record JiraWorklogRawCsv
(
    string IssueKey,
    string Date,
    string TimeSpent,
    string TempoWorklogType, // 2do? rework to more generic "Tempo worklog attributes"
    string Comment
)
{ }
