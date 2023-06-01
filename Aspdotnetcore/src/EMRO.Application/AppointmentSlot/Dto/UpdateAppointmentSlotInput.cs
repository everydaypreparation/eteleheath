using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class UpdateAppointmentSlotInput
    {
        public Guid Id { get; set; }
        public int TenantId { get; set; }
        public Guid UserId { get; set; }
        public string AvailabilityDate { get; set; }
        public string AvailabilityStartTime { get; set; }
        public string AvailabilityEndTime { get; set; }
        public string TimeZone { get; set; }
        public Guid UpdatedBy { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
