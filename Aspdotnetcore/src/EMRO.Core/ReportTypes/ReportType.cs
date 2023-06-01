using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.ReportTypes
{
    public class ReportType : Entity<Guid>, IMayHaveTenant
    {
        [Column("ReportTypeId")]
        [Key]
        public override Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? TenantId { get; set; }
    }
}
