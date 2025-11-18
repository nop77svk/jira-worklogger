namespace jwl.Wadl
{
    using System.Xml.Serialization;

    [XmlRoot("request")]
    public class WadlResourceRequest
    {
        [XmlElement("param")]
        public WadlParameter[]? Parameters { get; set; }

        [XmlElement("representation")]
        public WadlRepresentation[]? Representations { get; set; }
    }
}
