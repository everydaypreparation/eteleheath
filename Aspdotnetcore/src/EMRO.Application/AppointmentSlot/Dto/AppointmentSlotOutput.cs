using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.AppointmentSlot.Dto
{
    public class AppointmentSlotOutput
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class AppointmentSlotAvailableOutput
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
