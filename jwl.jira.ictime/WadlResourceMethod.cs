using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace jwl.jira.ictime
{
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
