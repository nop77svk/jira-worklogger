namespace jwl.jira.api.rest.request;

public class TempoFindWorklogs
{
    public common.TempoDate From { get; }
    public common.TempoDate To { get; }

    public string[]? IssueKey { get; init; }
    public string[]? UserKey { get; init; }

    public TempoFindWorklogs(DateOnly from, DateOnly to)
    {
        From = new common.TempoDate(from);
        To = new common.TempoDate(to);
    }
}
