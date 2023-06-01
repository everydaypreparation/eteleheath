using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.MeetingStatistics.Dto
{
    public class MeetingLogInput
    {
        public int? TenantId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid MeetingId { get; set; }
    }
}
