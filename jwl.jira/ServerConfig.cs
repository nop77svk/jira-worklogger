namespace jwl.jira;
using System.ComponentModel;

public class ServerConfig
{
    public string? BaseUrl { get; init; }
    public bool? UseProxy { get; init; }
    public int? MaxConnectionsPerServer { get; init; }
    public bool? SkipSslCertificateCheck { get; init; }
}
