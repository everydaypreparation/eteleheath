using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class BookAppointmentOutput
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
