using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class UpdateAppointmentInput
    {
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? TenantId { get; set; }
        public int AppointmentFrom { get; set; }
        public int AppointmentWith { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantEmail { get; set; }
        public string ParticipantType { get; set; }
        public string ParticipantRole { get; set; }
        public string Status { get; set; }
        public int Flag { get; set; }
        public long AppointmentId { get; set; }
        public long Id { get; set; }
    }
}
