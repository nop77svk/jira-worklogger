namespace jwl.jira.api.rest.request;

public class JiraAddWorklogByIssueKey
{
    public string? Started { get; init; }
    public int? TimeSpentSeconds { get; init; }
    public string? Comment { get; init; }
}
