namespace jwl.jira.Flavours;

public class VanillaJiraFlavourOptions
    : IFlavourOptions
{
    public string PluginBaseUri { get; init; } = @"rest/api/2";
}
