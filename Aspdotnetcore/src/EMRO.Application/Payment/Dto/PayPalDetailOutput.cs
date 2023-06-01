using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class PayPalDetailOutput
    {
        public string PayeeEmailAddress { get; set; }
        public string PaymentOrderId { get; set; }
        public double? PayAmount { get; set; }
        public Guid? CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public string PaymentStatus { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
