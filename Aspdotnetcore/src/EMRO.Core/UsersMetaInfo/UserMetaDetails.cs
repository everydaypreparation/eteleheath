using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.UsersMetaInfo
{
    public class UserMetaDetails : Entity<Guid>, IMayHaveTenant
    {
        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }

        [Column("UserDataId")]
        [Key]
        public override Guid Id { get; set; }

        public Guid? UserId { get; set; }
        //public string AffiliationPrimary { get; set; }
        //public string AffiliationSecondary { get; set; }
        public string CurrentAffiliation { get; set; }
        public string HospitalAffiliation { get; set; }
        public string ProfessionalBio { get; set; }
        public string UndergraduateMedicalTraining { get; set; }

        //public string PostGraduateAndResidencyTraining { get; set; }
        //public DateTime? DateCompleted { get; set; }
        public string OncologySpecialty { get; set; }
        public string OncologySubSpecialty { get; set; }
        public string MedicalAssociationMembership { get; set; }
        public string LicensingNumber { get; set; }
        //public string ParticipationInInsurancePlans { get; set; }
        //public string Choice1 { get; set; }
        //public string Choice2 { get; set; }
        //public string PaymentOption { get; set; }
        public string Company { get; set; }
        //public string ConsentForSharingAndAccessingMedicalInformation { get; set; }

        public string ConsentForSharingAndAccessingMedicalInformationWithDoctors { get; set; }
        public string RequestedOncologySubspecialty { get; set; }
        //public string UploadedDocumentType { get; set; }
        //public string UploadedDocumentSize { get; set; }
        //public string TotalNumberOfPagesUploaded { get; set; }
        public DateTime? DateConfirmed { get; set; }

        // Patient

        //public string ReasonForConsult { get; set; }
        //public string DiseaseDetails { get; set; }

        //public string DoctorFirstName { get; set; }
        //public string DoctorLastName { get; set; }
        //public string DoctorAddress { get; set; }
        //public string DoctorCity { get; set; }
        //public string DoctorState { get; set; }
        //public string DoctorCountry { get; set; }
        //public string DoctorPostalCodes { get; set; }

        //public string DoctorTelePhone { get; set; }
        //public string DoctorEmailID { get; set; }
        public string ConsentMedicalInformationWithCancerCareProvider { get; set; }

        //public string ReportTypeId { get; set; }
        //public string ReportPath { get; set; }
        public Guid? FamilyDoctorId { get; set; }
        public Guid? InviteUserId { get; set; }
        public bool IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? TenantId { get; set; }

        public string ConsultationType { get; set; }
        public string Certificate { get; set; }
        public string Residency1 { get; set; }
        public string Residency2 { get; set; }
        public string Fellowship { get; set; }
        public string ExperienceOrTraining { get; set; }
        public string Credentials { get; set; }

        public string AdminNotes { get; set; }

        public double AmountDeposit { get; set; }
    }
}
