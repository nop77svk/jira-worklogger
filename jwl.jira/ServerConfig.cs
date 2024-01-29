namespace jwl.jira;

public class ServerConfig
{
    public string? ServerFlavour { get; init; }
    public JiraServerFlavour ServerFlavourId => ServerApiFactory.DecodeServerClass(ServerFlavour) ?? JiraServerFlavour.Vanilla;
    public Dictionary<string, string>? ActivityMap { get; init; }
    public string? BaseUrl { get; init; }
    public bool? UseProxy { get; init; }
    public int? MaxConnectionsPerServer { get; init; }
    public bool? SkipSslCertificateCheck { get; init; }
}
