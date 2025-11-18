namespace jwl.Jira.Contract.Rest.Common;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TempoWorklogAttributeTypeIdentifier
{
    [EnumMember(Value = "STATIC_LIST")]
    StaticList
}
