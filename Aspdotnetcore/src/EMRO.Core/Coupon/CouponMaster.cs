using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Coupon
{
    public class CouponMaster : Entity<Guid>, IMayHaveTenant
    {
        [Column("CouponId")]
        [Key]
        public override Guid Id { get; set; }
        public int? TenantId { get; set; }
        public string DiscountCode { get; set; }
        public string DiscountPercent { get; set; }
        public DateTime CouponStart { get; set; }
        public DateTime CouponExpire { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
