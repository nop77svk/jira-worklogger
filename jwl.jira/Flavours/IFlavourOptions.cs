namespace jwl.jira.Flavours;

public interface IFlavourOptions
{
    string PluginBaseUri { get; init; }
    string DateFormat { get; init; }
    string TimeSpanFormat { get; init; }
}
