using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class UpcomingBookAppointmentDto
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string AppointmentDate { get; set; }
        public string StartTime { get; set; }
        public Guid? SlotId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public string TimeZone { get; set; }

        public DateTime SlotStartTime { get; set; }
        public DateTime SlotEndTime { get; set; }
        public string meetingId { get; set; }
        public string BookedBy { get; set; }

        public string Title { get; set; }

        public Guid PatientId { get; set; }

        public bool IsBookingResechdule { get; set; }
        public bool IsJoinMeeting { get; set; }

        public string RoleName { get; set; }

    }
}
