using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class PayPalInput
    {
        public string PayeeEmailAddress { get; set; }
        public string PayeeMerchantId { get; set; }
        public string PayerFullName { get; set; }
        public string PayerEmailAddress { get; set; }
        public string PayerId { get; set; }
        public double? OriginalPayAmount { get; set; }
        public string CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
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

        public bool IsPatient { get; set; }
    }
}
