using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.SubSpecialty
{
    public class SubSpecialtyMaster : Entity<Guid>, IMayHaveTenant
    {
        [Column("SubSpecialityId")]
        [Key]
        public override Guid Id { get; set; }
        public string SubSpecialityName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }

        public int? TenantId { get; set; }
    }
}
