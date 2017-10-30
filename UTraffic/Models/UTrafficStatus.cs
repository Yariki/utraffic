using System.Net;

namespace UklonTraffic.Models
{
    public class UTrafficStatus
    {
        public UTrafficStatus()
        {
        }

        public int RegionCode { get; set; }

        public string Title { get; set; }

        public int TrafficValue { get; set; } = -1;

        public eUStatus Status { get; set; }

        public override string ToString()
        {
            return $"{Title} - {RegionCode} {TrafficValue} - {Status}";
        }
    }
}