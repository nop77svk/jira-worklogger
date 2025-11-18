namespace jwl.Jira.Contract.Rest.Response;

public class JiraIssueWorklogs
{
    public JiraIssueWorklogs(int startAt, int maxResults, int total, JiraIssueWorklogsWorklog[] worklogs)
    {
        StartAt = startAt;
        MaxResults = maxResults;
        Total = total;
        Worklogs = worklogs;
    }

    public int StartAt { get; }
    public int MaxResults { get; }
    public int Total { get; }
    public JiraIssueWorklogsWorklog[] Worklogs { get; }
}