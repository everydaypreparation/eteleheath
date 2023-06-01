using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Users.Dto
{
    public class GetUserDtoOutput
    {
        public ConsuntantListOutput Consuntant { get; set; }
        public PatientListOutput Patient { get; set; }
        public FamilyDoctorListOutput FamilyDoctor { get; set; }
        public DaignosticListOutput Daignostic { get; set; }
        public InsuranceListOutput Insurance { get; set; }
        public MedicalLegalListOutput MedicalLegal { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }

        //public void Normalize()
        //{
        //    if (RoleNames == null)
        //    {
        //        RoleNames = new string[0];
        //    }
        //}
    }

    public class ConsuntantListOutput
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }

        public virtual string PhoneNumber { get; set; }

        public string CurrentAffiliation { get; set; }
        public string HospitalAffiliation { get; set; }
        public string ProfessionalBio { get; set; }
        public string UndergraduateMedicalTraining { get; set; }
        public string OncologySpecialty { get; set; }
        public string[] OncologySubSpecialty { get; set; }
        public string MedicalAssociationMembership { get; set; }
        public string LicensingNumber { get; set; }
        public DateTime? DateConfirmed { get; set; }
        public string[] ConsultationType { get; set; }
        public string Certificate { get; set; }
        public string Residency1 { get; set; }
        public string Residency2 { get; set; }
        public string Fellowship { get; set; }
        public string ExperienceOrTraining { get; set; }
        public string Credentials { get; set; }
        public string AdminNotes { get; set; }

    }

    public class PatientListOutput
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }

        public virtual string PhoneNumber { get; set; }

        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorAddress { get; set; }
        public string DoctorCity { get; set; }
        public string DoctorState { get; set; }
        public string DoctorCountry { get; set; }
        public string DoctorPostalCodes { get; set; }
        public string DoctorTelePhone { get; set; }
        public string DoctorEmailID { get; set; }
        public string ConsentMedicalInformationWithCancerCareProvider { get; set; }
        public Guid FamilyDoctorId { get; set; }
        public string FamilyDoctorTitle { get; set; }
        public string AdminNotes { get; set; }
        public string HospitalAffiliation { get; set; }
        public string OncologySpecialty { get; set; }
        public string familydoctorUploadProfilePicture { get; set; }

    }

    public class FamilyDoctorListOutput
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }

        public virtual string PhoneNumber { get; set; }

        public string CurrentAffiliation { get; set; }
        public string HospitalAffiliation { get; set; }
        public string ProfessionalBio { get; set; }
        public string UndergraduateMedicalTraining { get; set; }
        public string OncologySpecialty { get; set; }
        public string MedicalAssociationMembership { get; set; }
        public string LicensingNumber { get; set; }
        public DateTime? DateConfirmed { get; set; }
        public string Certificate { get; set; }
        public string Residency1 { get; set; }
        public string Residency2 { get; set; }
        public string Fellowship { get; set; }
        public string ExperienceOrTraining { get; set; }
        public string Credentials { get; set; }
        public string AdminNotes { get; set; }

    }

    public class DaignosticListOutput
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }
        public virtual string PhoneNumber { get; set; }
        public string AdminNotes { get; set; }

    }

    public class InsuranceListOutput
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }

        public virtual string PhoneNumber { get; set; }

        public string Company { get; set; }

        public string RequestedOncologySubspecialty { get; set; }
        public string AdminNotes { get; set; }
        public double AmountDeposit { get; set; }

    }

    public class MedicalLegalListOutput
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public virtual string UploadProfilePicture { get; set; }
        public string EmailAddress { get; set; }

        public virtual string PhoneNumber { get; set; }

        public string Company { get; set; }

        public string RequestedOncologySubspecialty { get; set; }
        public string AdminNotes { get; set; }
        public double AmountDeposit { get; set; }

    }
}
