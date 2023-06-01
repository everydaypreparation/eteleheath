using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class PaymentDetailOutput
    {
        public string NameOnCard { get; set; }
        public string CardNumber { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public double? PayAmount { get; set; }
        public string CardType { get; set; }
        //public int Fee { get; set; }
        public double? OriginalPayAmount { get; set; }
        public Guid? CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public string CardOrigin { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
