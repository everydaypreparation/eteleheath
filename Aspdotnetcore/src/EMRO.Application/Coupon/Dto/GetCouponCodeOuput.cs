using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Coupon.Dto
{
    public class GetCouponCodeOuput
    {
        public List<CouponMasterDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class ValidCoupon {

        public string Amount { get; set; }
        public string DiscountAmount { get; set; }
        public bool Is100PercentAmount { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
