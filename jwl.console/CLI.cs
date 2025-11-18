#pragma warning disable S101
namespace jwl.Console;

using CommandLine;

[Verb("fill", isDefault: true, HelpText = "Fill Jira with worklogs")]
public class FillCLI
{
    [Option('i', "input", HelpText = "\nInput CSVs with the worklogs", Separator = ',', Required = true)]
    public IEnumerable<string> InputFiles { get; set; } = new string[0];

    [Option("ifs", HelpText = "Input CSV fields delimiter"
        + $"\nJSON config: $.{nameof(Core.AppConfig.CsvOptions)}.{nameof(Inputs.CsvFormatConfig.FieldDelimiter)}")]
    public string? FieldDelimiter { get; set; }

    [Option('t', "target", HelpText = "Connection string to Jira server in the form of <user name>@<server host>[:<server port>]"
        + "\nNote: The \"https://\" scheme is automatically asserted with this option!"
        + $"\nJSON config: $.{nameof(Core.AppConfig.JiraServer)}.{nameof(Jira.ServerConfig.BaseUrl)} for <server host>[:<server port>]"
        + $"\nJSON config: $.{nameof(Core.AppConfig.User)}.{nameof(Core.UserConfig.Name)} for <user name>")]
    public string? UserCredentials { get; set; }

    [Option("server-flavour", HelpText = "Jira server flavour (whether vanilla or with some timesheet plugins)"
        + $"\nJSON config: $.{nameof(Core.AppConfig.JiraServer)}.{nameof(Jira.ServerConfig.Flavour)}"
        + $"\nAvailable values: {nameof(Jira.JiraServerFlavour.Vanilla)}, {nameof(Jira.JiraServerFlavour.TempoTimeSheets)}, {nameof(Jira.JiraServerFlavour.ICTime)}")]
    public string? ServerFlavour { get; set; }

    [Option("no-proxy", HelpText = "Turn off proxying the HTTP(S) connections to Jira server"
        + $"\nJSON config: $.{nameof(Core.AppConfig.JiraServer)}.{nameof(Jira.ServerConfig.UseProxy)} (negated!)")]
    public bool? NoProxy { get; set; }

    [Option("max-connections-per-server", HelpText = "Limit the number of concurrent connections to Jira server"
        + $"\nJSON config: $.{nameof(Core.AppConfig.JiraServer)}.{nameof(Jira.ServerConfig.MaxConnectionsPerServer)}")]
    public int? MaxConnectionsPerServer { get; set; }

    [Option("no-ssl-cert-check", HelpText = "Skip SSL/TLS certificate checks (for self-signed certificates)"
        + $"\nJSON config: $.{nameof(Core.AppConfig.JiraServer)}.{nameof(Jira.ServerConfig.SkipSslCertificateCheck)}")]
    public bool? SkipSslCertificateCheck { get; set; }

    public Core.AppConfig ToAppConfig()
    {
        string[] connectionSpecifierSplit = UserCredentials?.Split('@', 2, StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

        string? jiraServerSpecification = connectionSpecifierSplit.Length > 1 ? connectionSpecifierSplit[1] : null;
        if (!jiraServerSpecification?.Contains(@"://") ?? false)
        {
            jiraServerSpecification = @"https://" + jiraServerSpecification;
        }

        string? jiraUserCredentials = connectionSpecifierSplit.Any() ? connectionSpecifierSplit[0] : null;
        string[] jiraUserCredentialsSplit = jiraUserCredentials?.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        string? jiraUserName = jiraUserCredentialsSplit.Any() ? jiraUserCredentialsSplit[0] : null;
        string? jiraUserPassword = jiraUserCredentialsSplit.Length > 1 ? jiraUserCredentialsSplit[1] : null;

        return new Core.AppConfig()
        {
            JiraServer = new Jira.ServerConfig()
            {
                Flavour = ServerFlavour,
                FlavourOptions = null,
                BaseUrl = jiraServerSpecification,
                UseProxy = !NoProxy,
                MaxConnectionsPerServer = MaxConnectionsPerServer,
                SkipSslCertificateCheck = SkipSslCertificateCheck
            },
            User = new Core.UserConfig()
            {
                Name = jiraUserName,
                Password = jiraUserPassword
            },
            CsvOptions = new Inputs.CsvFormatConfig()
            {
                FieldDelimiter = FieldDelimiter
            }
        };
    }
}