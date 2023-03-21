namespace jwl.jira.api.rest.response;

public class JiraIssueWorklogs
{
    public int? StartAt { get; init; }
    public int? MaxResults { get; init; }
    public int? Total { get; init; }
    public JiraIssueWorklogsWorklog[]? Worklogs { get; init; }
}
