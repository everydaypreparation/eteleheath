using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.MeetingStatistics.Dto
{
    public class MeetingLogOutput
    {
        public Guid MeetingLogId { get; set; }
        public int? TenantId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid MeetingId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }
}
