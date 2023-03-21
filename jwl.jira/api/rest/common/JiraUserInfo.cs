namespace jwl.jira.api.rest.common;
using System.Text.Json.Serialization;

public class JiraUserInfo
{
    public string? Name { get; init; }
    public string? Key { get; init; }
    public string? EmailAddress { get; init; }
    public string? DisplayName { get; init; }
    [JsonPropertyName("active")]
    public bool? IsActive { get; init; }
    [JsonPropertyName("deleted")]
    public bool? Deleted { get; init; }
    public string? TimeZone { get; init; }
    public string? Locale { get; init; }
}
