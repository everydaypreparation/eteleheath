using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class CreatePatientInput
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
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
        public List<IFormFile> ReportDocumentPaths { get; set; }
        public List<IFormFile> RadiationDocumentPaths { get; set; }
        public List<IFormFile> OtherDocumentPaths { get; set; }
        public IFormFile Signature { get; set; }

        public string DateConfirmation { get; set; }

        public Guid UserId { get; set; }

        // public int? TenantId { get; set; }

        public bool IsPatient { get; set; }

        public Guid? FamilyDoctorId { get; set; }

        public int NoOfPages { get; set; }

        public Guid UserConsentId { get; set; }

        public string[] ConsultantReportsIds { get; set; }
        public string RelationshipWithPatient { get; set; }
        public string RepresentativeFirstName { get; set; }
        public string RepresentativeLastName { get; set; }

    }

    public class UpdatePatientInput
    {
        [Required]
        public Guid UserConsentId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TelePhone { get; set; }
        [Required]
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
        public List<IFormFile> ReportDocumentPaths { get; set; }
        public List<IFormFile> RadiationDocumentPaths { get; set; }
        public List<IFormFile> OtherDocumentPaths { get; set; }
        public IFormFile Signature { get; set; }

        public string DateConfirmation { get; set; }

        public Guid UserId { get; set; }

        // public int? TenantId { get; set; }

        public bool IsPatient { get; set; }

        public Guid? FamilyDoctorId { get; set; }

        public int NoOfPages { get; set; }

    }


    //public class Uploadfiles
    //{
    //    public string Category { get; set; }
    //    public List<IFormFile> DocumentPaths { get; set; }

    //}
}
