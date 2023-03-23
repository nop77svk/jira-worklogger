namespace jwl.config;

public class ConfigLocationResolver
{
    public string PreferredAppSubfolder { get; }

    public ConfigLocationResolver(string preferredAppSubfolder)
    {
        if (string.IsNullOrWhiteSpace(preferredAppSubfolder))
            throw new ArgumentOutOfRangeException(nameof(preferredAppSubfolder), "Cannot be null nor empty nor whitespace-only");

        PreferredAppSubfolder = preferredAppSubfolder;
    }

    public string Resolve(SymbolicConfigLocation location)
    {
        return location switch
        {
            SymbolicConfigLocation.ApplicationFolder => AppDomain.CurrentDomain.BaseDirectory,
            SymbolicConfigLocation.UserLocalSettingsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PreferredAppSubfolder),
            SymbolicConfigLocation.UserRoamingSettingsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PreferredAppSubfolder),
            SymbolicConfigLocation.CurrentFolder => Path.GetFullPath("."),
            _ => throw new ArgumentOutOfRangeException(location.ToString())
        };
    }
}
