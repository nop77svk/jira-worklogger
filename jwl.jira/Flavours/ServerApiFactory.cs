namespace jwl.jira.Flavours;

public static class ServerApiFactory
{
    public static IJiraClient CreateApi(HttpClient httpClient, string userName, ServerConfig? serverConfig)
    {
        FlavourVanillaJiraOptions vanillaJiraOptions = serverConfig?.VanillaJiraFlavourOptions ?? new FlavourVanillaJiraOptions();
        VanillaJiraClient vanillaJiraClient = new VanillaJiraClient(httpClient, userName, vanillaJiraOptions);

        JiraServerFlavour serverFlavour = serverConfig?.FlavourId ?? JiraServerFlavour.Vanilla;
        IFlavourOptions? flavourOptions = serverConfig?.FlavourOptions;

        try
        {
            return serverFlavour switch
            {
                JiraServerFlavour.Vanilla => new VanillaJiraClient(httpClient, userName, vanillaJiraOptions),
                JiraServerFlavour.TempoTimeSheets => new JiraWithTempoPluginApi(httpClient, userName, vanillaJiraClient, (FlavourTempoTimesheetsOptions?)flavourOptions),
                JiraServerFlavour.ICTime => new JiraWithICTimePluginApi(httpClient, userName, vanillaJiraClient, (FlavourICTimeOptions?)flavourOptions),
                _ => throw new NotImplementedException($"Jira server flavour {nameof(serverFlavour)} not yet implemented")
            };
        }
        catch (InvalidCastException ex)
        {
            throw new ArgumentOutOfRangeException($"Don't know how to instantiate client API for flavour {serverFlavour}", ex);
        }
    }

    public static JiraServerFlavour? DecodeServerClass(string? serverFlavour)
    {
        JiraServerFlavour? result;

        if (string.IsNullOrEmpty(serverFlavour))
            result = null;
        else if (int.TryParse(serverFlavour, out int serverFlavourIntId))
            result = (JiraServerFlavour)serverFlavourIntId;
        else if (Enum.TryParse(serverFlavour, true, out JiraServerFlavour serverFlavourEnumId))
            result = serverFlavourEnumId;
        else
            throw new ArgumentOutOfRangeException(nameof(serverFlavour), serverFlavour, "Invalid server flavour configured");

        return result;
    }
}
