using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class AppointmentDetails
    {
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? TenantId { get; set; }
        public long AppointmentFrom { get; set; }
        public long AppointmentWith { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantEmail { get; set; }
        public string ParticipantType { get; set; }
        public string ParticipantRole { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long DeletedBy { get; set; }
        public DateTime DeletedOn { get; set; }
    }
}
