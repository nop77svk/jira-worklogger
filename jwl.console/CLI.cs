namespace jwl.console;
using CommandLine;

[Verb("fill", isDefault: true, HelpText = "Fill Jira with worklogs")]
public class FillCLI
{
    [Option('i', "input", HelpText = "Input CSVs with the worklogs", Separator = ',', Required = true)]
    public IEnumerable<string> InputFiles { get; set; } = new string[0];

    [Option("ifs", HelpText = "Input CSV field delimiter")]
    public string? FieldDelimiter { get; set; }

    [Option('u', "user", HelpText = "Credentials+server for Jira in the form of <user name>@<server host>[:<server port>]")]
    public string? UserCredentials { get; set; }

    [Option("no-proxy", HelpText = "Turn off proxying the HTTP(S) connections to Jira server")]
    public bool? NoProxy { get; set; }

    [Option("max-connections-per-server", HelpText = "Limit the number of concurrent connections to Jira server")]
    public int? MaxConnectionsPerServer { get; set; }

    [Option("no-ssl-cert-check", HelpText = "Skip SSL/TLS certificate checks (for self-signed certificates)")]
    public bool? SkipSslCertificateCheck { get; set; }

    public core.AppConfig ToAppConfig()
    {
        string[] connectionSpecifierSplit = UserCredentials?.Split('@', 2, StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

        string? jiraServerSpecification = connectionSpecifierSplit.Length > 1 ? connectionSpecifierSplit[1] : null;
        if (!jiraServerSpecification?.Contains(@"://") ?? false)
            jiraServerSpecification = @"https://" + jiraServerSpecification;
        /*
        string[] jiraServerSpecificationSplit = jiraServerSpecification?.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();
        string? jiraServerHost = jiraServerSpecificationSplit.Any() ? jiraServerSpecificationSplit[0] : null;
        string? jiraServerPortStr = jiraServerSpecificationSplit.Length > 1 ? jiraServerSpecificationSplit[1] : null;
        int? jiraServerPort;
        try
        {
            jiraServerPort = string.IsNullOrEmpty(jiraServerPortStr) ? null : int.Parse(jiraServerPortStr);
        }
        catch (Exception e)
        {
            throw new ArgumentOutOfRangeException($"Invalid Jira server port {jiraServerPortStr}", e);
        }
        */

        string? jiraUserCredentials = connectionSpecifierSplit.Any() ? connectionSpecifierSplit[0] : null;
        string[] jiraUserCredentialsSplit = jiraUserCredentials?.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        string? jiraUserName = jiraUserCredentialsSplit.Any() ? jiraUserCredentialsSplit[0] : null;
        string? jiraUserPassword = jiraUserCredentialsSplit.Length > 1 ? jiraUserCredentialsSplit[1] : null;

        return new core.AppConfig()
        {
            JiraServer = new jira.ServerConfig()
            {
                BaseUrl = jiraServerSpecification,
                UseProxy = !NoProxy,
                MaxConnectionsPerServer = MaxConnectionsPerServer,
                SkipSslCertificateCheck = SkipSslCertificateCheck
            },
            User = new core.UserConfig()
            {
                Name = jiraUserName,
                Password = jiraUserPassword
            },
            CsvOptions = new inputs.CsvFormatConfig()
            {
                FieldDelimiter = FieldDelimiter
            }
        };
    }
}