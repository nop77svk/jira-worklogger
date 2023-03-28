namespace jwl.core;
using jwl.inputs;
using jwl.jira;
using Microsoft.Extensions.Configuration;

public static class AppConfigFactory
{
    public static readonly AppConfig DefaultConfig = new AppConfig()
    {
        JiraServer = new ServerConfig()
        {
            BaseUrl = @"https://jira.at-my-company.org:7777",
            MaxConnectionsPerServer = 4,
            UseProxy = false,
            SkipSslCertificateCheck = false
        },
        CsvOptions = new CsvFormatConfig()
        {
            FieldDelimiter = ","
        },
        User = new UserConfig()
        {
            Name = string.Empty,
            Password = null
        }
    };

    private const string ConfigFileName = @"jwl.config";
    private const string DefaultSubFolder = @"jira-worklogger";

    public static AppConfig ReadConfig()
    {
        AppConfig? result;

        IConfiguration config = new ConfigurationBuilder()
            // 2do! command line custom provider
            .AddJsonFile(Path.Combine(Path.GetFullPath("."), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName), optional: true)
            .Build();

        result = config.Get<AppConfig>(opt =>
        {
            opt.BindNonPublicProperties = false;
            opt.ErrorOnUnknownConfiguration = true;
        });

        return result ?? DefaultConfig;
    }
}
