namespace jwl.config;

public class ConfigLocationResolver
{
    public string PreferredAppSubfolder { get; }
    public bool UseAssemblyFolder { get; init; } = true;
    public bool UseCurrentFolder { get; init; } = true;
    public bool UseUserProfileFolders { get; init; } = true;

    public ConfigLocationResolver(string preferredAppSubfolder)
    {
        if (string.IsNullOrWhiteSpace(preferredAppSubfolder))
            throw new ArgumentOutOfRangeException(nameof(preferredAppSubfolder), "Cannot be null nor empty nor whitespace-only");

        PreferredAppSubfolder = preferredAppSubfolder;
    }

    public IEnumerable<string> GetDefaultConfigFolders(IEnumerable<Environment.SpecialFolder> specialFolders)
    {
        if (UseCurrentFolder)
            yield return Path.GetFullPath(".");

        foreach (Environment.SpecialFolder folder in specialFolders)
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PreferredAppSubfolder);

        if (UseAssemblyFolder)
            yield return AppDomain.CurrentDomain.BaseDirectory;
    }

    public IEnumerable<string> GetDefaultConfigFolders()
    {
        Environment.SpecialFolder[] specialFolders = UseUserProfileFolders switch
        {
            true => new Environment.SpecialFolder[]
            {
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolder.ApplicationData
            },
            false => Array.Empty<Environment.SpecialFolder>()
        };

        return GetDefaultConfigFolders(specialFolders);
    }
}
