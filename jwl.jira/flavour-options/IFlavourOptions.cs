namespace jwl.jira.FlavourOptions;

public interface IFlavourOptions
{
    string PluginBaseUri { get; init; }
    Dictionary<string, string>? ActivityMap { get; init; }
}
