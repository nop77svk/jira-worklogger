#pragma warning disable SA1313
namespace jwl.jira.api.rest.request;

public record ICTimeAddWorklogByIssueKey(string Started, int TimeSpentSeconds, int? Activity, string? Comment)
{
}
