namespace jwl.jira.api.rest.response;

public class JiraIssueWorklogsWorklog
{
    public common.JiraStringedInteger? Id { get; init; }
    public common.JiraStringedInteger? IssueId { get; init; }
    public common.JiraUserInfo? Author { get; init; }
    public common.JiraUserInfo? UpdateAuthor { get; init; }
    public string? Comment { get; init; }
    public common.JiraTimeStamp? Created { get; init; }
    public common.JiraTimeStamp? Updated { get; init; }
    public common.JiraTimeStamp? Started { get; init; }
    public string? TimeSpent { get; init; }
    public int? TimeSpentSeconds { get; init; }
}
