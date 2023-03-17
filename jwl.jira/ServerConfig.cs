namespace jwl.jira;

// 2do! config
public class ServerConfig
{
    public string BaseUrl { get; init; } = @"https://jira.ri-rpc.corp:8080";
    public bool UseProxy { get; init; } = false;
    public int MaxConnectionsPerServer { get; init; } = 4;
    public bool SkipSslCertificateCheck { get; init; } = true;
}
