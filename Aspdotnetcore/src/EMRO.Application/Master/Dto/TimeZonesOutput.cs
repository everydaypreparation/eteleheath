using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Master.Dto
{
    public class TimeZonesOutput
    {
        public List<TimeZonesListOutput> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class TimeZonesListOutput
    {
        public string TimeZoneId { get; set; }
        public string UTCOffset { get; set; }
        public string Abbr { get; set; }
    }
}
