#pragma warning disable SA1313
namespace Jwl.Jira.api.rest.request;

public record JiraAddWorklogByIssueKey(string Started, int TimeSpentSeconds, string? Comment)
{
}
