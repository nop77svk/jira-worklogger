#pragma warning disable SA1313
namespace jwl.inputs;

public struct JiraWorklogRawCsv
{
    public string IssueKey;
    public string Date;
    public string TimeSpent;
    public string TempoWorklogType; // 2do? rework to more generic "Tempo worklog attributes"
    public string Comment;
}
