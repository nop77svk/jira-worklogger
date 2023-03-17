namespace jwl.jira.api.rest.response;

public struct JiraIssueWorklogsWorklog
{
    public long Id;
    public long IssueId;
    public common.JiraUserInfo Author;
    public common.JiraUserInfo UpdateAuthor;
    public string Comment;
    public DateTime Created;
    public DateTime Updated;
    public DateTime Started;
    public string TimeSpent;
    public int TimeSpentSeconds;
}
