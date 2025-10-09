#pragma warning disable S1104
namespace Jwl.Inputs;

using Jwl.Infra;

public struct InputWorkLog
{
    public JiraIssueKey IssueKey;
    public DateTime Date;
    public TimeSpan TimeSpent;
    public string WorkLogActivity;
    public string WorkLogComment;
}
