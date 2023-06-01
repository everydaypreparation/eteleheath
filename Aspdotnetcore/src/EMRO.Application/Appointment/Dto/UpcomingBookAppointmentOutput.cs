using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class UpcomingBookAppointmentOutput
    {
        public List<UpcomingBookAppointmentDto> Items { get; set; }
        public int Count { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

    public class UpcomingBookAppointmentInput
    {
        public Guid Id { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

    public class BookAppointmentInput
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

}
