#pragma warning disable SA1313
namespace jwl.jira.api.rest.request;

public record ICTimeAddWorklogByIssueKey(string Started, int TimeSpentSeconds, int? Activity, string? Comment)
{
    public string LogWorkOption { get; init; } = "summary";
    public string AdjustEstimate { get; init; } = "auto";
    public bool ActivateLogwork { get; init; } = true;
}
