using System.Xml.Serialization;

namespace jwl.jira.ictime
{
    [XmlRoot("representation")]
    public class WadlRepresentation
    {
        [XmlAttribute("mediaType")]
        public string? MediaType { get; set; }

        [XmlElement("param")]
        public WadlResourceParameter[]? Parameters { get; set; }
    }
}