using System;
using System.Xml.Serialization;

namespace UklonTraffic.Models
{
    [Serializable()]
    [XmlRoot("info")]
    public class UInfo
    {
        [XmlElement("traffic")]
        public UTraffic Traffic { get; set; }
    }
}