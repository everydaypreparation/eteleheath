using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class GetAppoinmentDetailsOutput
    {
        public Guid? PatientId { get; set; }
        public long PatientIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string Country { get; set; }
        //public string PostalCode { get; set; }
        public string TelePhone { get; set; }
        public string EmailID { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ProfileUrl { get; set; }

        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorTitle { get; set; }
        public string DoctorSpecialty { get; set; }
        public string HospitalName { get; set; }
        public Guid AppointmentId { get; set; }
        public string DoctorProfileUrl { get; set; }
        public string Id { get; set; }
        public Guid LegalId { get; set; }
        public string LegalProfileUrl { get; set; }
        public string LegalCompany { get; set; }
        public string LegalName { get; set; }
        public string RoleName { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string meetingId { get; set; }
        public Guid DoctorId { get; set; }

        public Guid ConsultId { get; set; }

        public bool IsArchived { get; set; }
        public bool IsPayment { get; set; }

       
    }

    public class GetPatientAppoinmentDetailsOutput
    {
        public Guid SlotId { get; set; }
        public string AppointmentDate { get; set; }
        public string StartTime { get; set; }
        public string TimeZone { get; set; }
        public string UserName { get; set; }
        public Guid DoctorId { get; set; }

        public DateTime SlotStartTime { get; set; }
        public DateTime SlotEndTime { get; set; }

        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorTitle { get; set; }
        public string DoctorSpecialty { get; set; }
        public string HospitalName { get; set; }
        public Guid AppointmentId { get; set; }
        public string DoctorProfileUrl { get; set; }

        public string FamilyDoctorFirstName { get; set; }
        public string FamilyDoctorLastName { get; set; }
        public string FamilyDoctorTitle { get; set; }
        public string FamilyDoctorSpecialty { get; set; }
        public string FamilyHospitalName { get; set; }
        public string FamilyDoctorProfileUrl { get; set; }

        public Guid FamilyDoctorId { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string meetingId { get; set; }

        public Guid PatientId { get; set; }

        public bool IsBookingResechdule { get; set; }
        public bool IsJoinMeeting { get; set; }

        public string RoleName { get; set; }
    }
}
