using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class CancelBookedAppointmentInput
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Reason { get; set; }
    }
}
