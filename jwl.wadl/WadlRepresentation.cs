namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("representation")]
    public class WadlRepresentation
    {
        public enum MediaTypes
        {
            [XmlEnum("text/plain")]
            PlainText,
            [XmlEnum("application/xml")]
            Xml,
            [XmlEnum("application/json")]
            Json,
            [XmlEnum("application/atom+xml")]
            AtomXml,
            [XmlEnum("application/x-www-form-urlencoded")]
            WwwFormUrlEncoded
        }

        [XmlAttribute("mediaType")]
        public MediaTypes MediaType { get; set; }

        [XmlElement("param")]
        public WadlParameter[]? Parameters { get; set; }
    }
}