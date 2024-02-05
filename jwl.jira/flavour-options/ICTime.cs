#pragma warning disable SA1313
namespace jwl.jira.FlavourOptions;
using System.Collections.Generic;

public class ICTime
    : IFlavourOptions
{
    public string PluginBaseUri { get; init; } = "rest/ictime/1.0";
    public string DateFormat { get; init; } = "dd/MMM/yyyy";
    public string TimeSpanFormat { get; init; } = "ddd\" d \"hh\" h \"mm\" m\"";
    public Dictionary<string, string>? ActivityMap { get; init; }
}
