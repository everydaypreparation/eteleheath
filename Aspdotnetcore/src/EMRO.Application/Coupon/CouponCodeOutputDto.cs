using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Coupon.Dto
{
    public class CouponCodeOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}