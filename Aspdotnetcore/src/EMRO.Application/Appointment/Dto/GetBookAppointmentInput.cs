using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class GetBookAppointmentInput
    {
        public Guid? AppointmentId { get; set; }
        public Guid? UserId { get; set; }
    }
}
