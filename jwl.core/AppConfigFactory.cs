namespace jwl.core;
using jwl.inputs;
using jwl.jira;
using Microsoft.Extensions.Configuration;

public static class AppConfigFactory
{
    private const string ConfigFileName = @"jwl.config";
    private const string DefaultSubFolder = @"jira-worklogger";

    private static readonly AppConfig EmptyConfig = new AppConfig()
    {
        JiraServer = new ServerConfig()
        {
            BaseUrl = ServerConfig.Default_BaseUrl,
            MaxConnectionsPerServer = ServerConfig.Default_MaxConnectionsPerServer,
            SkipSslCertificateCheck = ServerConfig.Default_SkipSslCertificateCheck,
            UseProxy = ServerConfig.Default_UseProxy
        },
        CsvOptions = new CsvFormatConfig()
        {
            FieldDelimiter = CsvFormatConfig.Default_FieldDelimiter
        },
        User = new UserConfig()
        {
            Name = UserConfig.Default_Name,
            Password = UserConfig.Default_Password
        }
    };

    public static AppConfig ReadConfig()
    {
        AppConfig? result;

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName), optional: true)
            .AddJsonFile(Path.Combine(Path.GetFullPath("."), ConfigFileName), optional: true)
            // 2do! custom command line config provider at the highest priority
            .Build();

        result = config.Get<AppConfig>(opt =>
        {
            opt.BindNonPublicProperties = false;
            opt.ErrorOnUnknownConfiguration = true;
        });

        return result ?? EmptyConfig;
    }
}
