namespace jwl.jira;

public static class ServerApiFactory
{
    public static IJiraServerApi CreateApi(HttpClient httpClient, string userName, JiraServerFlavour serverClass)
    {
        return serverClass switch
        {
            JiraServerFlavour.Vanilla => new VanillaJiraServerApi(httpClient, userName),
            JiraServerFlavour.TempoTimeSheets => new JiraWithTempoPluginApi(httpClient, userName),
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
