using System;
using System.Xml.Serialization;

namespace UklonTraffic.Models
{
    [Serializable]
    public class URegion
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("length")]
        public float Length { get; set; }

        [XmlElement("level")]
        public int Level { get; set; }

        [XmlElement("time")]
        public string Time { get; set; }

    }
}