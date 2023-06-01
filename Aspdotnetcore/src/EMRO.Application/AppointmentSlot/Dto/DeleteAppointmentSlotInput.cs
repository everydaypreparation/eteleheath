using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class DeleteAppointmentSlotInput
    {
        public Guid Id { get; set; }
    }

    public class AvailableAppointmentSlotInput
    {
        public Guid AppointmentId { get; set; }
    }
}
