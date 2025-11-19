namespace jwl.Wadl
{
    using System.Xml.Serialization;

    [XmlRoot("application", Namespace = WadlApplication.XmlNamespace)]
    public class WadlApplication
    {
        internal const string XmlNamespace = "http://wadl.dev.java.net/2009/02";

        [XmlArray("resources")]
        [XmlArrayItem("resource")]
        public WadlResource[]? Resources { get; set; }
    }
}