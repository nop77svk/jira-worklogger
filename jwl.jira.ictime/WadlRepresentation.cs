namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("representation")]
    public class WadlRepresentation
    {
        [XmlAttribute("mediaType")]
        public string? MediaType { get; set; }

        [XmlElement("param")]
        public WadlResourceParameter[]? Parameters { get; set; }
    }
}