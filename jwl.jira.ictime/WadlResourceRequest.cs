using System.Xml.Serialization;

namespace jwl.jira.ictime
{
    [XmlRoot("request")]
    public class WadlResourceRequest
    {
        [XmlElement("param")]
        public WadlResourceParameter[]? Parameters { get; set; }

        [XmlElement("representation")]
        public WadlRepresentation[]? Representations { get; set; }
    }
}