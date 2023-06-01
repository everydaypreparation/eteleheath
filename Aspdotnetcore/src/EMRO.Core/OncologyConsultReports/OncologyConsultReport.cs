using Abp.Domain.Entities;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.OncologyConsultReports
{
    public class OncologyConsultReport : Entity<Guid>, IMayHaveTenant
    {
        [Column("ConsultId")]
        [Key]
        public override Guid Id { get; set; }
        public string Purpose { get; set; }
        public string Allergies { get; set; }
        public string Investigation { get; set; }
        public string Impression { get; set; }
        public string Plan { get; set; }
        public string FamilyHistory { get; set; }
        public string SocialHistory { get; set; }
        public string Medication { get; set; }
        public string PastMedicalHistory { get; set; }
        public string Notes { get; set; }
        public string ReviewOfHistory { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? TenantId { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual DoctorAppointment DoctorAppointment { get; set; }
        public Guid? AppointmentId { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string SignaturePath { get; set; }
        public string ReportPath { get; set; }

        public bool IsBlobStorage { get; set; }
        public string SignatureRequestId { get; set; }
        public string ReportRequestId { get; set; }
    }
}
