#pragma warning disable SA1313

namespace jwl.Inputs;

using jwl.Infra;

public struct InputWorkLog
{
    public JiraIssueKey IssueKey { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public string WorkLogActivity { get; set; }
    public string WorkLogComment { get; set; }
}