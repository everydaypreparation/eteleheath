using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Coupon.Dto
{
    public class CreateCouponCodeInputDto
    {
        public string DiscountCode { get; set; }
        public string DiscountPercent { get; set; }
        public DateTime CouponStart { get; set; }
        public DateTime CouponExpire { get; set; }
	}
}