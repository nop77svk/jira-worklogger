#pragma warning disable SA1313
namespace jwl.Jira.Contract.Rest.Request;

public record JiraAddWorklogByIssueKey(string Started, int TimeSpentSeconds, string? Comment)
{
}
