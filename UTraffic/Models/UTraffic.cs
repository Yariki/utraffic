using System;
using System.Xml;
using System.Xml.Serialization;

namespace UklonTraffic.Models
{
    [Serializable]
    public class UTraffic
    {
        [XmlAttribute("region")]
        public int Region { get; set; }

        [XmlAttribute("zoom")]
        public int Zoom { get; set; }

        [XmlAttribute("lat")]
        public float Lat { get; set; }

        [XmlAttribute("lon")]
        public float Lon { get; set; }

        [XmlElement("region")]
        public URegion RegionInfo { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }


    }
}