using Abp.Domain.Entities;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using EMRO.InviteUsers;
using EMRO.Master;
using EMRO.Patients;
using EMRO.Patients.IntakeForm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.UserConsents
{
    public class UserConsent : Entity<Guid>,IMayHaveTenant
    {
        [ForeignKey("UserConsentPatientsDetailsId")]
        public virtual UserConsentPatientsDetails  UserConsentPatientsDetails { get; set; }

        //[ForeignKey("UserConsentFamilyDoctorsDetailsId")]
        //public virtual UserConsentFamilyDoctorsDetails UserConsentFamilyDoctorsDetails { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual DoctorAppointment  DoctorAppointments { get; set; }

        [ForeignKey("UserConsentFormId")]
        public virtual UserConsentForm UserConsentForm { get; set; }

        [Column("UserConsentsId")]
        [Key]
        public override Guid Id { get; set; }

        public string ConsentMedicalInformationWithCancerCareProvider { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }

        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public Guid UserConsentPatientsDetailsId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? UserConsentFormId { get; set; }

        //public Guid? UserConsentFamilyDoctorsDetailsId { get; set; }
        public Guid? AppointmentId { get; set; }
        public int? TenantId { get; set; }
        public Guid? FamilyDoctorId { get; set; }
        public Guid? PatientId { get; set; }

        [ForeignKey("InviteUserId")]
        public virtual InviteUser  InviteUser { get; set; }

        public Guid? InviteUserId { get; set; }
    }
}
