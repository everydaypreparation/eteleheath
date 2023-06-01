using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EMRO.Patients.IntakeForm
{
    public class UserConsentPatientsDetails : Entity<Guid>, IMayHaveTenant
    {
        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }

        [Column("UserConsentPatientsDetailsId")]
        [Key]
        public override Guid Id { get; set; }

        [Column(TypeName = "bytea")]
        public string FirstName { get; set; }

        [Column(TypeName = "bytea")]
        public string LastName { get; set; }

        [Column(TypeName = "bytea")]
        public string Address { get; set; }

        [Column(TypeName = "bytea")]
        public string City { get; set; }
        [Column(TypeName = "bytea")]
        public string State { get; set; }
        [Column(TypeName = "bytea")]
        public string Country { get; set; }
        [Column(TypeName = "bytea")]
        public string PostalCode { get; set; }

        [Column(TypeName = "bytea")]
        public string TelePhone { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string EmailID { get; set; }
        [Column(TypeName = "bytea")]
        public string DateOfBirth { get; set; }

        [Column(TypeName = "bytea")]
        public string Gender { get; set; }

        public string ReasonForConsult { get; set; }
        public string DiseaseDetails { get; set; }

        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }

        public Guid? UserId { get; set; }
        public int? TenantId { get; set; }
        [NotMapped]
        public string CaseId
        {
            get
            {
                return string.Concat("C-", Convert.ToString(CaseNumber).Count(char.IsDigit) == 1 ? "000" + CaseNumber :
                Convert.ToString(CaseNumber).Count(char.IsDigit) == 2 ? "00" + CaseNumber :
                Convert.ToString(CaseNumber).Count(char.IsDigit) == 3 ? "0" + CaseNumber : "C-" + CaseNumber);
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CaseNumber { get; set; }
        public string ConsultantReportsIds { get; set; }
        public string RelationshipWithPatient { get; set; }
        public string RepresentativeFirstName { get; set; }
        public string RepresentativeLastName { get; set; }
    }
}
