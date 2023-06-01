using Abp.Domain.Entities;
using EMRO.Coupon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Payment
{
    public class UserAppointmentPayment : Entity<Guid>, IMayHaveTenant
    {
        [ForeignKey("CouponId")]
        public virtual CouponMaster CouponMaster { get; set; }

        [Column("PaymentId")]
        [Key]
        public override Guid Id { get; set; }
        public virtual int? TenantId { get; set; }
        public string NameOnCard { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string CardOrigin { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public double? OriginalPayAmount { get; set; }
        public double? PayAmount { get; set; }
        public int Fee { get; set; }
        public Guid? CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string PaymentMessage { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }

        public int PaymentMethod { get; set; }

        public string PayeeEmailAddress { get; set; }
        public string PayeeMerchantId { get; set; }
        public string PayerFullName { get; set; }
        public string PayerEmailAddress { get; set; }
        public string PayerId { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public DateTime PaymentCreateTime { get; set; }
        public DateTime PaymentUpdateTime { get; set; }
        public string PaymentOrderId { get; set; }
        public string PaymentStatus { get; set; }
        public string PayerAddressLine { get; set; }
        public string PayerAdminArea2 { get; set; }
        public string PayerAdminArea1 { get; set; }
        public string PayerPostalCode { get; set; }
        public string PayerCountryCode { get; set; }

    }
}
