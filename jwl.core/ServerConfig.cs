namespace jwl.core;

// 2do! config
public class ServerConfig
{
    public string BaseUrl { get; init; } = @"https://jira.ri-rpc.corp:8080";
    public bool UseProxy { get; init; } = false;
    public int MaxConnectionsPerServer { get; init; } = 8;
    public bool SkipSslCertificateCheck { get; init; } = true;
    public string JiraUserName { get; init; } = "hrapet";
    public string? JiraUserPassword { get; init; } = null;
}
