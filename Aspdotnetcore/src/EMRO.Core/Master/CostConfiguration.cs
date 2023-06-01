using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Master
{
    public class CostConfiguration : Entity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public int? TenantId { get; set; }
        [Key]
        [Column("CostId")]
        public override Guid Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        //public string RoleName { get; set; }
        //public double ConsultationFee { get; set; }
        //public double BaseRate { get; set; }
        //public double PerPageRate { get; set; }
        //public int UptoPages { get; set; }

        public string KeyName { get; set; }
        public string Value { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
