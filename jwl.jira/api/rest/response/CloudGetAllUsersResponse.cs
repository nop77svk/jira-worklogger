namespace jwl.jira.api.rest.response;

using System.Text.Json.Serialization;

public class CloudFindUsersResponseElement
{
    public string? AccountId { get; init; }
    public string? AccountType { get; init; }
    public bool Active { get; init; }
    public AvatarUrlsNested? AvatarUrls { get; init; }
    public string? DisplayName { get; init; }
    public string? Key { get; init; }
    public string? Name { get; init; }
    public string? Self { get; init; }

    public sealed class AvatarUrlsNested
    {
        [JsonPropertyName("16x16")]
        public string? AvatarUrl16x16 { get; init; }

        [JsonPropertyName("24x24")]
        public string? AvatarUrl24x24 { get; init; }

        [JsonPropertyName("32x32")]
        public string? AvatarUrl32x32 { get; init; }

        [JsonPropertyName("48x48")]
        public string? AvatarUrl48x48 { get; init; }
    }
}
