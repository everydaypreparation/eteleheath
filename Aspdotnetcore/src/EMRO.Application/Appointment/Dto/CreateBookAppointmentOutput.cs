using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class CreateBookAppointmentOutput
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public long OriginalPaymentAmount { get; set; }
    }

    // Temp for managing consulation fee
    public class ConsultFee
    {
        public long Amount { get; set; }
    }
}
