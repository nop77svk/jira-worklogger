namespace jwl.console;
using CommandLine;

[Verb("fill", isDefault: true, HelpText = "Fill Jira with worklogs")]
public class FillCLI
{
    [Option('v', "verbose", HelpText = "\nGive more verbose feedback\nNote: Not implemented yet! 2do! :-)", Default = false, Hidden = true)]
    public bool UseVerboseFeedback { get; set; }

    [Option('i', "input", HelpText = "\nInput CSVs with the worklogs", Separator = ',', Required = true)]
    public IEnumerable<string> InputFiles { get; set; } = new string[0];

    [Option("ifs", HelpText = "Input CSV fields delimiter"
        + $"\nJSON config: $.{nameof(core.AppConfig.CsvOptions)}.{nameof(inputs.CsvFormatConfig.FieldDelimiter)}")]
    public string? FieldDelimiter { get; set; }

    [Option('t', "target", HelpText = "Connection string to Jira server in the form of <user name>@<server host>[:<server port>]"
        + "\nNote: The \"https://\" scheme is automatically asserted with this option!"
        + $"\nJSON config: $.{nameof(core.AppConfig.JiraServer)}.{nameof(jira.ServerConfig.BaseUrl)} for <server host>[:<server port>]"
        + $"\nJSON config: $.{nameof(core.AppConfig.User)}.{nameof(core.UserConfig.Name)} for <user name>")]
    public string? UserCredentials { get; set; }

    [Option("server-class", HelpText = "Jira server class (whether vanilla or with timesheet plugins)"
        + $"\nJSON config: $.{nameof(core.AppConfig.JiraServer)}.{nameof(jira.ServerConfig.ServerClass)}"
        + $"\nAvailable values: {nameof(jira.JiraServerClass.VanillaJira)}, {nameof(jira.JiraServerClass.TempoTimeSheetsPlugin)}, {nameof(jira.JiraServerClass.ICTimePlugin)}")]
    public string? ServerClass { get; set; }

    [Option("no-proxy", HelpText = "Turn off proxying the HTTP(S) connections to Jira server"
        + $"\nJSON config: $.{nameof(core.AppConfig.JiraServer)}.{nameof(jira.ServerConfig.UseProxy)} (negated!)")]
    public bool? NoProxy { get; set; }

    [Option("max-connections-per-server", HelpText = "Limit the number of concurrent connections to Jira server"
        + $"\nJSON config: $.{nameof(core.AppConfig.JiraServer)}.{nameof(jira.ServerConfig.MaxConnectionsPerServer)}")]
    public int? MaxConnectionsPerServer { get; set; }

    [Option("no-ssl-cert-check", HelpText = "Skip SSL/TLS certificate checks (for self-signed certificates)"
        + $"\nJSON config: $.{nameof(core.AppConfig.JiraServer)}.{nameof(jira.ServerConfig.SkipSslCertificateCheck)}")]
    public bool? SkipSslCertificateCheck { get; set; }

    public core.AppConfig ToAppConfig()
    {
        string[] connectionSpecifierSplit = UserCredentials?.Split('@', 2, StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

        string? jiraServerSpecification = connectionSpecifierSplit.Length > 1 ? connectionSpecifierSplit[1] : null;
        if (!jiraServerSpecification?.Contains(@"://") ?? false)
            jiraServerSpecification = @"https://" + jiraServerSpecification;

        string? jiraUserCredentials = connectionSpecifierSplit.Any() ? connectionSpecifierSplit[0] : null;
        string[] jiraUserCredentialsSplit = jiraUserCredentials?.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        string? jiraUserName = jiraUserCredentialsSplit.Any() ? jiraUserCredentialsSplit[0] : null;
        string? jiraUserPassword = jiraUserCredentialsSplit.Length > 1 ? jiraUserCredentialsSplit[1] : null;

        return new core.AppConfig()
        {
            UseVerboseFeedback = UseVerboseFeedback,
            JiraServer = new jira.ServerConfig()
            {
                ServerClass = ServerClass,
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