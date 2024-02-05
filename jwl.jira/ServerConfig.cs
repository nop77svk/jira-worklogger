namespace jwl.jira;
using System.Text.Json.Serialization;

public class ServerConfig
{
    public string? BaseUrl { get; init; }
    public string? Flavour { get; init; }
    public JiraServerFlavour FlavourId => ServerApiFactory.DecodeServerClass(Flavour) ?? JiraServerFlavour.Vanilla;

    [JsonConverter(typeof(FlavourOptionsCustomConverter))]
    public object? FlavourOptions { get; init; }
    public bool? UseProxy { get; init; }
    public int? MaxConnectionsPerServer { get; init; }
    public bool? SkipSslCertificateCheck { get; init; }
}
