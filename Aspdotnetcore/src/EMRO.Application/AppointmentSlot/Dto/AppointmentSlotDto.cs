using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class AppointmentSlotDto
    {
        public Guid Id { get; set; }
        public virtual int? TenantId { get; set; }
        public Guid UserId { get; set; }
        public string AvailabilityDate { get; set; }
        public string AvailabilityStartTime { get; set; }
        public string AvailabilityEndTime { get; set; }
        public string TimeZone { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int Flag { get; set; }
        public string Status { get; set; }
        public int IsBooked { get; set; }

        public DateTime SlotStartTime { get; set; }
        public DateTime SlotEndTime { get; set; }

    }
}
