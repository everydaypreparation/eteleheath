using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class GetDoctorAppointmentSlot
    {
        public Guid UserId { get; set; }
        public string AvailabilityDate { get; set; }
        public string AvailabilityStartTime { get; set; }
        public string AvailabilityEndTime { get; set; }
        public string TimeZone { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public Guid Id { get; set; }
    }
}
