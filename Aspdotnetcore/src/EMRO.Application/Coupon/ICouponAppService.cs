using Abp.Application.Services;
using EMRO.Coupon.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Coupon
{
    public interface ICouponAppService : IApplicationService
    {
        Task<GetCouponCodeOuput> Get();
        Task<ValidCoupon> ValidateCoupon(CouponInputDto input);
    }
}
