using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class UpcomingPatientAppointmentOutput
    {
        public List<UpcomingPatientAppointmentDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
