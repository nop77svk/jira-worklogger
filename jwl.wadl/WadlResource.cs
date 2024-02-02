namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("resource", Namespace = WadlApplication.XmlNamespace)]
    public class WadlResource
    {
        [XmlAttribute("path")]
        public string? Path { get; set; }

        [XmlElement("param")]
        public WadlResourceParameter[]? Parameters { get; set; }

        [XmlElement("method")]
        public WadlResourceMethod[]? Methods { get; set; }

        [XmlElement("resource")]
        public WadlResource[]? Resources { get; set; }
    }
}