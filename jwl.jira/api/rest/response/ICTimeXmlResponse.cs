namespace jwl.Jira.api.rest.response;
using System.Xml.Serialization;

[XmlRoot("result")]
public class ICTimeXmlResponse
{
    [XmlAttribute("status")]
    public string? Status { get; init; }

    [XmlElement("success")]
    public string? Success { get; init; }
}
