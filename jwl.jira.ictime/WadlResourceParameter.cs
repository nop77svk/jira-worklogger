namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("param", Namespace = WadlApplication.XmlNamespace)]
    public class WadlResourceParameter
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("style")]
        public string? Style { get; set; }
        
        [XmlAttribute("type")]
        public string? Type { get; set; }

        [XmlAttribute("default")]
        public string? Default { get; set; }
    }
}
