using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.DiagnosticsCases
{
    public class DiagnosticsCase : Entity<Guid>, IMayHaveTenant, ISoftDelete
    {
        [Column("CaseId")]
        [Key]
        public override Guid Id { get; set; }
        public int? TenantId { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid UserId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsCompleted { get; set; }
       
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }
    }
}
