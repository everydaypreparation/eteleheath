using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class DoctorAppointmentSlotOutput
    {
        public List<AppointmentSlotDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }

    public class DoctorAppointmentSlotInputDto
    {
        public Guid UserId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }
}
