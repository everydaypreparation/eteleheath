using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Coupon.Dto
{
    public class CouponInputDto
    {
        [Required]
        public string Coupon { get; set; }
        [Required]
        public double OriginalPayAmount { get; set; }
        public Guid UserId { get; set; }
    }
}
