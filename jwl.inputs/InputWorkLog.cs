#pragma warning disable SA1313
namespace jwl.inputs;
using jwl.infra;

public struct InputWorkLog
{
    public JiraIssueKey IssueKey;
    public DateTime Date;
    public TimeSpan TimeSpent;
    public string WorkLogActivity;
    public string WorkLogComment;
}
