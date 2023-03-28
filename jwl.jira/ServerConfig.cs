namespace jwl.jira;

public class ServerConfig
{
    public const string Default_BaseUrl = @"https://jira.at-my-company.org:7777";
    public const int Default_MaxConnectionsPerServer = 4;
    public const bool Default_UseProxy = false;
    public const bool Default_SkipSslCertificateCheck = false;

    public string? BaseUrl { get; init; }
    public bool? UseProxy { get; init; }
    public int? MaxConnectionsPerServer { get; init; }
    public bool? SkipSslCertificateCheck { get; init; }
}
