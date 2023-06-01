using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class StripeSetting
    {
        public string SecretKey { get; set; }
        public string PublicKey { get; set; }
    }
    public class ValidaToken
    {
        public int StatusCode { get; set; }
        public Token stripeToken { get; set; }
        public string Message { get; set; }
        public Charge charge { get; set; }

        public ChargeCreateOptions  options { get; set; }
    }
}
