using Abp.Domain.Entities;
using EMRO.Coupon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.UserCoupon
{
    public class UserCouponTransaction : Entity<Guid>, IMayHaveTenant
    {

        [ForeignKey("CouponId")]
        public virtual CouponMaster CouponMaster { get; set; }

        [Column("CouponTransactionId")]
        [Key]
        public override Guid Id { get; set; }
        public int? TenantId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public Guid? CouponId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
