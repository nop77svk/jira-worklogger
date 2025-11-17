namespace jwl.Wadl
{
    using System.Linq;
    using System.Xml.Serialization;

    [XmlRoot("param", Namespace = WadlApplication.XmlNamespace)]
    public class WadlParameter
    {
        public const string QueryStyle = "query";
        public const string TemplateStyle = "template";

        public const string BooleanType = "xs:boolean";
        public const string DoubleType = "xs:double";
        public const string IntType = "xs:int";
        public const string LongType = "xs:long";
        public const string StringType = "xs:string";

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
