using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Master
{
    public class TimeZones : Entity<Guid>, IMayHaveTenant
    {
        [Column("TimeZoneId")]
        [Key]
        public override Guid Id { get; set; }
        public string UTCOffset { get; set; }
        public string Abbr { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
