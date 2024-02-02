﻿namespace jwl.wadl
{
    using System.Xml.Serialization;

    [XmlRoot("request")]
    public class WadlResourceRequest
    {
        [XmlElement("param")]
        public WadlResourceParameter[]? Parameters { get; set; }

        [XmlElement("representation")]
        public WadlRepresentation[]? Representations { get; set; }
    }
}