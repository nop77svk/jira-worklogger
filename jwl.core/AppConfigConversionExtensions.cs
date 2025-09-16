namespace Jwl.Core;

using Jwl.Inputs;
using Jwl.Jira;

public static class AppConfigConversionExtensions
{
    public static ServerConfig OverrideWith(this ServerConfig self, ServerConfig? other)
        => other is null
        ? self
        : new ServerConfig()
        {
            BaseUrl = other.BaseUrl ?? self.BaseUrl,
            Flavour = other.Flavour ?? self.Flavour,
            FlavourOptions = self.FlavourOptions,
            MaxConnectionsPerServer = other.MaxConnectionsPerServer ?? self.MaxConnectionsPerServer,
            SkipSslCertificateCheck = other.SkipSslCertificateCheck ?? self.SkipSslCertificateCheck,
            UseProxy = other.UseProxy ?? self.UseProxy,
            VanillaJiraFlavourOptions = self.VanillaJiraFlavourOptions
        };

    public static UserConfig OverrideWith(this UserConfig self, UserConfig? other)
        => other is null
        ? self
        : new UserConfig()
        {
            Name = other.Name ?? self.Name,
            Password = other.Password ?? self.Password
        };

    public static CsvFormatConfig OverrideWith(this CsvFormatConfig self, CsvFormatConfig? other)
        => other is null
        ? self
        : new CsvFormatConfig()
        {
            FieldDelimiter = other.FieldDelimiter ?? self.FieldDelimiter,
        };
}
