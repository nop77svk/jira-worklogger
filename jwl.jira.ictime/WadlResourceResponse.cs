using System.Xml.Serialization;

namespace jwl.jira.ictime
{
    [XmlRoot("response")]
    public class WadlResourceResponse
    {
        [XmlElement("representation")]
        public WadlRepresentation[]? Representations { get; set; }
    }
}