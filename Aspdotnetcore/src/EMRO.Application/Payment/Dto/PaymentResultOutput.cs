using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Payment.Dto
{
    public class PaymentResultOutput
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public Guid PaymentId { get; set; }
    }
}
