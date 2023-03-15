namespace jwl.core.api.rest.common;
using System.Text.Json.Serialization;

public struct JiraUserInfo
{
    public string Name;
    public string Key;
    public string EmailAddress;
    public string DisplayName;
    [JsonPropertyName("active")]
    public bool IsActive;
    [JsonPropertyName("deleted")]
    public bool? Deleted;
    public string TimeZone;
    public string? Locale;
}
