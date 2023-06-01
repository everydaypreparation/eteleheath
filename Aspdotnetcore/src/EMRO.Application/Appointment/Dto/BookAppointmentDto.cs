using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class BookAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public long AppointmentFrom { get; set; } // logged in user
        public long AppointmentWith { get; set; } // select doctor
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string[] AdditionalAttendee { get; set; } // user participants
        public Guid CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        public int Flag { get; set; }
        public string Status { get; set; }
        public string meetingId { get; set; }
    }
}
