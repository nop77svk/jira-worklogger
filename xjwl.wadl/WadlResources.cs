namespace jwl.Wadl
{
    using System.Xml.Serialization;

    public class WadlResources
    {
        [XmlAttribute("base")]
        public string? UriBase { get; set; }

        [XmlArrayItem("resource")]
        public WadlResource[]? Children { get; set; }
    }
}