namespace jwl.Wadl
{
    using System.Xml.Serialization;

    [XmlRoot("representation")]
    public class WadlRepresentation
    {
        public const string MediaTypePlainText = @"text/plain";
        public const string MediaTypeXml = @"application/xml";
        public const string MediaTypeAtomXml = @"application/atom+xml";
        public const string MediaTypeJson = @"application/json";
        public const string MediaTypeFormUrlEncoded = @"application/x-www-form-urlencoded";

        [XmlAttribute("mediaType")]
        public MediaTypes MediaType { get; set; }

        [XmlElement("param")]
        public WadlParameter[]? Parameters { get; set; }

        public enum MediaTypes
        {
            [XmlEnum(MediaTypePlainText)]
            PlainText,

            [XmlEnum(MediaTypeXml)]
            Xml,

            [XmlEnum(MediaTypeJson)]
            Json,

            [XmlEnum(MediaTypeAtomXml)]
            AtomXml,

            [XmlEnum(MediaTypeFormUrlEncoded)]
            WwwFormUrlEncoded
        }
    }
}
