using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.MeetingStatistics.Dto
{
    public class MeetingLogCountOutput
    {
        public Guid MeetingId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }
}
