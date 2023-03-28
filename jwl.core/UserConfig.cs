namespace jwl.core;

public class UserConfig
{
    public const string Default_Name = @"";
    public const string? Default_Password = null;

    public string? Name { get; init; }
    public string? Password { get; init; }
}
