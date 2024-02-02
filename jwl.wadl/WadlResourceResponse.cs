﻿namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("response")]
    public class WadlResourceResponse
    {
        [XmlElement("representation")]
        public WadlRepresentation[]? Representations { get; set; }
    }
}