#pragma warning disable SA1313
#pragma warning disable S1104

namespace jwl.Inputs;

using jwl.Infra;

public struct InputWorkLog
{
    public JiraIssueKey IssueKey;
    public DateTime Date;
    public TimeSpan TimeSpent;
    public string WorkLogActivity;
    public string WorkLogComment;
}