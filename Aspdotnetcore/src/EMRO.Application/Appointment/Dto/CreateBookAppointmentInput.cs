using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class CreateBookAppointmentInput
    {
        public string Title { get; set; }
        public string Agenda { get; set; }
        //public long PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid SlotId { get; set; }
        //public long AppointmentFrom { get; set; }
        //public long AppointmentWith { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        //public DateTime AppointmentDate { get; set; }
        //public string[] AdditionalAttendee { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        //public int Flag { get; set; }
        public string Status { get; set; }
        public Guid UserId { get; set; }
        public string meetingId { get; set; }
    }

    public class UpdateBookInput
    {
        public Guid SlotId { get; set; }
        public Guid AppointmentId { get; set; }
    }
}
