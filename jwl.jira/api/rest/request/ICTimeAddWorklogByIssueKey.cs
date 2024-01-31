#pragma warning disable SA1313
namespace jwl.jira.api.rest.request;
using System.Text;
using System.Text.Json.Serialization;

public record ICTimeAddWorklogByIssueKey(string IssueKey, string Started, int TimeSpentSeconds, int? Activity, string? Comment)
{
    [JsonIgnore]
    public string NoCharge { get; } = "off";
    [JsonIgnore]
    public string? NoChargeInfo { get; } = null;
    [JsonIgnore]
    public string LogWorkOption { get; } = "summary";
    [JsonIgnore]
    public string AdjustEstimate { get; } = "auto";
    [JsonIgnore]
    public bool ActivateLogwork { get; } = true;

    public string CustomFieldNNN => new StringBuilder()
        // noChargeInfo==why this should not be charged||timeSpentCorrected==5h||newEstimate==5w||startDate==Sep 29, 2014 9:34 AM||startTime==5:05 PM||endTime==5:07 PM||
        .Append($"adjustEstimate=={AdjustEstimate}||")
        .Append($"nocharge=={NoCharge}||")
        .Append(!string.IsNullOrEmpty(NoChargeInfo) ? $"noChargeInfo=={NoCharge}||" : string.Empty)
        .Append(Activity is not null ? $"activity=={Activity}||" : string.Empty)
        .Append(Comment is not null ? $"comment=={Comment}||" : string.Empty)
        .Append($"issueKey=={IssueKey}||")
        .Append($"logWorkOption=={LogWorkOption}||")
        .Append($"timeLogged=={TimeSpan.FromSeconds(TimeSpentSeconds).ToString()}||") // 2do!
        .Append($"activateLogwork=={ActivateLogwork.ToString().ToLower()}||")
        .ToString();
}
