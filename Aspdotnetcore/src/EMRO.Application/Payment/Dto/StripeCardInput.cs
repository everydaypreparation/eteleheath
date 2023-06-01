using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class StripeCardInput
    {
        public string NameOnCard { get; set; }
        public string CardNumber { get; set; }
        public string CVC { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        //public long PayAmount { get; set; }
        //public string CardType { get; set; }
        //public int Fee { get; set; }
        public double OriginalPayAmount { get; set; }
        public double PayAmount { get; set; }
        public string CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }

        public bool IsPatient { get; set; }
    }

    public class PaymentDetails
    {
        public Guid PaymentId { get; set; }
    }

    public class PayByDepositeInput
    {
        public double OriginalPayAmount { get; set; }
        public double PayAmount { get; set; }
        public string CouponId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }

        public bool IsPatient { get; set; }
    }
}
