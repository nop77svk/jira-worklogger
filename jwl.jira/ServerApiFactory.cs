namespace jwl.jira;

public static class ServerApiFactory
{
    public enum ServerClass
    {
        VanillaJira = 0,
        TempoTimeSheetsPlugin = 1,
        ICTimePlugin = 2
    }

    public static IJiraServerApi CreateApi(HttpClient httpClient, string userName, ServerClass serverClass)
    {
        return serverClass switch
        {
            ServerClass.VanillaJira => new VanillaJiraServerApi(httpClient, userName),
            ServerClass.TempoTimeSheetsPlugin => new JiraWithTempoPluginApi(httpClient, userName),
            _ => throw new NotImplementedException($"Jira server class {nameof(serverClass)} not yet implemented")
        };
    }
}
