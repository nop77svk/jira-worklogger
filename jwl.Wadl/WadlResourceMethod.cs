namespace Jwl.Wadl
{
    using System.Xml.Serialization;

    [XmlRoot("method")]
    public class WadlResourceMethod
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlAttribute("name")]
        public string? CallMethod { get; set; }

        [XmlElement("request")]
        public WadlResourceRequest? Request { get; set; }

        [XmlElement("response")]
        public WadlResourceResponse? Response { get; set; }
    }
}
