namespace jwl.Jira.Flavours;

public class FlavourTempoTimesheetsOptions
    : IFlavourOptions
{
    public string PluginBaseUri { get; init; } = @"rest/tempo-timesheets/4";
    public string PluginCoreUri { get; init; } = @"rest/tempo-core/1";
}
