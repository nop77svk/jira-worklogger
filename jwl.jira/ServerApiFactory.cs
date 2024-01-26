namespace jwl.jira;

public static class ServerApiFactory
{
    public static IJiraServerApi CreateApi(HttpClient httpClient, string userName, JiraServerClass serverClass)
    {
        return serverClass switch
        {
            JiraServerClass.VanillaJira => new VanillaJiraServerApi(httpClient, userName),
            JiraServerClass.TempoTimeSheetsPlugin => new JiraWithTempoPluginApi(httpClient, userName),
            _ => throw new NotImplementedException($"Jira server class {nameof(serverClass)} not yet implemented")
        };
    }

    public static JiraServerClass? DecodeServerClass(string? serverClass)
    {
        JiraServerClass? result;

        if (string.IsNullOrEmpty(serverClass))
            result = null;
        else if (int.TryParse(serverClass, out int serverClassIntId))
            result = (JiraServerClass)serverClassIntId;
        else if (Enum.TryParse(serverClass, true, out JiraServerClass serverClassEnumId))
            result = serverClassEnumId;
        else
            throw new ArgumentOutOfRangeException(nameof(serverClass), serverClass, "Invalid server class configured");

        return result;
    }
}
