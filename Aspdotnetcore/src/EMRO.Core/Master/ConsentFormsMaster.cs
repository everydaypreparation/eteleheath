using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Master
{
    public class ConsentFormsMaster : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Column("ConsentFormsId")]
        [Key]
        public override Guid Id { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubTitle { get; set; }
        public string ShortDescription { get; set; }

        public string Disclaimer { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
