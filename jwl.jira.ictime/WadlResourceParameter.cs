using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace jwl.jira.ictime
{
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
