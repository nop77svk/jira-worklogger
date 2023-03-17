namespace jwl.jira.api.rest.response;

public struct JiraIssueWorklogsWorklog
{
    public common.JiraStringedInteger Id;
    public common.JiraStringedInteger IssueId;
    public common.JiraUserInfo Author;
    public common.JiraUserInfo UpdateAuthor;
    public string Comment;
    public common.JiraTimeStamp Created;
    public common.JiraTimeStamp Updated;
    public common.JiraTimeStamp Started;
    public string TimeSpent;
    public int TimeSpentSeconds;
}
