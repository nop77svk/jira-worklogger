using System.Xml.Serialization;

namespace jwl.jira.ictime
{
    public class WadlResources
    {
        [XmlAttribute("base")]
        public string? UriBase { get; set; }

        [XmlArrayItem("resource")]
        public WadlResource[]? Children { get; set; }
    }
}
