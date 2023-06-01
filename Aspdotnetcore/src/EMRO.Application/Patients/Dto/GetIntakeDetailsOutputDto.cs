using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
   public class GetIntakeDetailsOutputDto
    {
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TelePhone { get; set; }
        public string EmailID { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ReasonForConsult { get; set; }
        public string DiseaseDetails { get; set; }

        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorAddress { get; set; }
        public string DoctorCity { get; set; }
        public string DoctorState { get; set; }
        public string DoctorCountry { get; set; }
        public string DoctorPostalCodes { get; set; }

        public string DoctorTelePhone { get; set; }
        public string DoctorEmailID { get; set; }

        public string DoctorDateOfBirth { get; set; }
        public string DoctorGender { get; set; }
        public string ConsentMedicalInformationWithCancerCareProvider { get; set; }
        public string ReportTypeName { get; set; }
        public string ReportPath { get; set; }
        public Guid AppointmentId { get; set; }

        public Guid ConsentFormsMasterId { get; set; }
        public List<DocumentData> Items { get; set; }
        public string Signature { get; set; }

        public string DateConfirmation { get; set; }

        public Guid UserId { get; set; }
        public Guid? PatientId { get; set; }

        // public int? TenantId { get; set; }

        public bool IsPatient { get; set; }

        public Guid? FamilyDoctorId { get; set; }

        public int NoOfPages { get; set; }

        public Guid UserConsentId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

        public string ConsultId { get; set; }

        public string[] ConsultantReportsIds { get; set; }
        public string RelationshipWithPatient { get; set; }
        public string RepresentativeFirstName { get; set; }
        public string RepresentativeLastName { get; set; }
    }
}
