#pragma warning disable SA1313
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
