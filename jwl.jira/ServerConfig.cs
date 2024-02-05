namespace jwl.jira;
using System.Text.Json.Serialization;
using jwl.jira.Flavours;

public class ServerConfig
{
    public string? BaseUrl { get; init; }
    public string? Flavour { get; init; }
    public JiraServerFlavour FlavourId => ServerApiFactory.DecodeServerClass(Flavour) ?? JiraServerFlavour.Vanilla;

    [JsonIgnore]
    public IFlavourOptions? FlavourOptions { get; set; }
    public bool? UseProxy { get; init; }
    public int? MaxConnectionsPerServer { get; init; }
    public bool? SkipSslCertificateCheck { get; init; }
}
