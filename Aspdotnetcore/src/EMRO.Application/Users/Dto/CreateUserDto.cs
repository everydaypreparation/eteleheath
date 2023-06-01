using System;
using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using EMRO.Authorization.Users;
using Microsoft.AspNetCore.Http;

namespace EMRO.Users.Dto
{
    [AutoMapTo(typeof(User))]
    public class CreateUserDto : IShouldNormalize
    {
        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Title { get; set; }
        public IFormFile UploadProfilePicture { get; set; }
        public virtual string PhoneNumber { get; set; }

        public string CurrentAffiliation { get; set; }
        public string HospitalAffiliation { get; set; }
        public string ProfessionalBio { get; set; }
        public string UndergraduateMedicalTraining { get; set; }
        public string OncologySpecialty { get; set; }
        public string[] OncologySubSpecialty { get; set; }
        public string MedicalAssociationMembership { get; set; }
        public string LicensingNumber { get; set; }
        //public string Company { get; set; }
        //public string ConsentForSharingAndAccessingMedicalInformationWithDoctors { get; set; }
        //public string RequestedOncologySubspecialty { get; set; }
        public DateTime? DateConfirmed { get; set; }
        //public string DoctorFirstName { get; set; }
        //public string DoctorLastName { get; set; }
        //public string DoctorAddress1 { get; set; }
        //public string DoctorAddress2 { get; set; }
        //public string DoctorCity { get; set; }
        //public string DoctorState { get; set; }
        //public string DoctorCountry { get; set; }
        //public string DoctorPostalCodes { get; set; }
        //public string DoctorTelePhone { get; set; }
        //public string DoctorEmailID { get; set; }
        //public string ConsentMedicalInformationWithCancerCareProvider { get; set; }

        public string[] ConsultationType { get; set; }
        public string Certificate { get; set; }
        public string Residency1 { get; set; }
        public string Residency2 { get; set; }
        public string Fellowship { get; set; }
        public string ExperienceOrTraining { get; set; }
        public string AdminNotes { get; set; }
        public string Credentials { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }

    public class PatientCreateUserDto : IShouldNormalize
    {
        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Title { get; set; }
        public IFormFile UploadProfilePicture { get; set; }
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
        public string AdminNotes { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }

    public class FamilyCreateUserDto : IShouldNormalize
    {
        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Title { get; set; }
        public IFormFile UploadProfilePicture { get; set; }
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
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }

    public class DiagnosticCreateUserDto : IShouldNormalize
    {
        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Title { get; set; }
        public IFormFile UploadProfilePicture { get; set; }
        public virtual string PhoneNumber { get; set; }
        public string AdminNotes { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }

    public class InsuranceCreateUserDto : IShouldNormalize
    {
        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Title { get; set; }
        public IFormFile UploadProfilePicture { get; set; }
        public virtual string PhoneNumber { get; set; }

        public string Company { get; set; }
        public string AdminNotes { get; set; }

        public double AmountDeposit { get; set; }
        public string RequestedOncologySubspecialty { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }
}
