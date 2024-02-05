namespace jwl.jira;

public static class ServerApiFactory
{
    public static IJiraClient CreateApi(HttpClient httpClient, string userName, JiraServerFlavour serverClass, object? flavourOptions)
    {
        return serverClass switch
        {
            JiraServerFlavour.Vanilla => new VanillaJiraClient(httpClient, userName),
            JiraServerFlavour.TempoTimeSheets => new JiraWithTempoPluginApi(httpClient, userName),
            JiraServerFlavour.ICTime => new JiraWithICTimePluginApi(httpClient, userName, new ICTimeFlavourOptions()),
            _ => throw new NotImplementedException($"Jira server class {nameof(serverClass)} not yet implemented")
        };
    }

    public static JiraServerFlavour? DecodeServerClass(string? serverClass)
    {
        JiraServerFlavour? result;

        if (string.IsNullOrEmpty(serverClass))
            result = null;
        else if (int.TryParse(serverClass, out int serverClassIntId))
            result = (JiraServerFlavour)serverClassIntId;
        else if (Enum.TryParse(serverClass, true, out JiraServerFlavour serverClassEnumId))
            result = serverClassEnumId;
        else
            throw new ArgumentOutOfRangeException(nameof(serverClass), serverClass, "Invalid server class configured");

        return result;
    }
}
