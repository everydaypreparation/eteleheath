using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.Coupon.Dto;
using EMRO.UserCoupon;
using EMRO.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EMRO.Coupon
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class CouponAppService : ApplicationService, ICouponAppService
    {
        private readonly IRepository<CouponMaster, Guid> _couponMasterRepository;
        private readonly EmroAppSession _session;
        private readonly IRepository<UserCouponTransaction, Guid> _userCouponTransaction;

        public CouponAppService(IRepository<CouponMaster, Guid> couponMasterRepository, 
        EmroAppSession session,
        IRepository<UserCouponTransaction, Guid> userCouponTransaction)
        {
            _couponMasterRepository = couponMasterRepository;
            _userCouponTransaction = userCouponTransaction;
            _session = session;
        }

        /// <summary>
        /// Method to create coupon codes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<CouponCodeOutputDto> Create(CreateCouponCodeInputDto input)
        {
            CouponCodeOutputDto output = new CouponCodeOutputDto();
            try
            {
                Guid sessionUser = Guid.Empty;
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }

                if (_session.UniqueUserId != null)
                {
                    sessionUser = Guid.Parse(_session.UniqueUserId);
                }

                var coupon = new CouponMaster
                {
                    DiscountCode = input.DiscountCode,
                    DiscountPercent = input.DiscountPercent,
                    CouponStart = input.CouponStart,
                    CouponExpire = input.CouponExpire,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    CreatedBy = sessionUser,
                    UpdatedBy = sessionUser,
                    TenantId = TenantId
                };
                output.Id = await _couponMasterRepository.InsertAndGetIdAsync(coupon);
                output.Message = "Coupon code saved successfully";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Coupon code Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetCouponCodeOuput> Get()
        {
            GetCouponCodeOuput getCouponCodeOuput = new GetCouponCodeOuput();
            Logger.Info("Get All Coupon Codes: ");
            try
            {
                var result = await _couponMasterRepository.GetAllListAsync(x => x.IsActive == true);

                if (result.Count > 0)
                {
                    getCouponCodeOuput.Items = (from l in result.AsEnumerable()
                                                where Convert.ToDateTime(l.CouponStart).Date <= DateTime.UtcNow.Date && Convert.ToDateTime(l.CouponExpire).Date >= DateTime.UtcNow.Date
                                                select new CouponMasterDto
                                                {
                                                    DiscountCode = l.DiscountCode,
                                                    DiscountPercent = l.DiscountPercent,
                                                    CouponStart = l.CouponStart,
                                                    CouponExpire = l.CouponExpire
                                                }).ToList();

                    getCouponCodeOuput.Message = "Get all coupon codes.";
                    getCouponCodeOuput.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                getCouponCodeOuput.Message = "Error while fetching all coupon codes.";
                getCouponCodeOuput.StatusCode = 401;
                Logger.Error("Get All Coupon Codes Error" + ex.StackTrace);
            }

            return getCouponCodeOuput;
        }

        public async Task<ValidCoupon> ValidateCoupon(CouponInputDto input)
        {
            ValidCoupon validCoupon = new ValidCoupon();
            Logger.Info("Validate Coupon Codes: ");
            try
            {
                if (!string.IsNullOrEmpty(input.Coupon) && input.OriginalPayAmount > 0 && input.UserId != null && input.UserId != Guid.Empty)
                {
                    var result = await _couponMasterRepository.GetAllListAsync(x => x.IsActive == true);
                    if (result.Count > 0)
                    {
                        var valid = (from l in result.AsEnumerable()
                                     where
                                     l.DiscountCode == input.Coupon
                                     && Convert.ToDateTime(l.CouponStart).Date <= DateTime.UtcNow.Date
                                     && Convert.ToDateTime(l.CouponExpire).Date >= DateTime.UtcNow.Date
                                     select l).FirstOrDefault();
                        if (valid != null)
                        {
                            var isCouponUsed = await _userCouponTransaction.GetAllListAsync(x => x.CouponId == valid.Id && x.UserId == input.UserId);
                            if (isCouponUsed.Count > 0)
                            {
                                validCoupon.Message = "Coupon already used.";
                                validCoupon.StatusCode = 401;
                            }
                            else if (Convert.ToDateTime(valid.CouponExpire).Date < DateTime.UtcNow.Date)
                            {
                                validCoupon.Message = "The coupon code has expired.";
                                validCoupon.StatusCode = 401;
                            }
                            else
                            {
                                double discount = 0;
                                double discountPercent = Convert.ToDouble(100) - Convert.ToDouble(Regex.Replace(valid.DiscountPercent, @"^[$]|%$", string.Empty));
                                discount = (discountPercent / 100) * input.OriginalPayAmount;
                                validCoupon.Amount = Math.Round(discount, 2).ToString();
                                validCoupon.DiscountAmount = Math.Round(input.OriginalPayAmount - Math.Round(discount, 2)).ToString();
                                if (discountPercent == 0)
                                {
                                    validCoupon.Is100PercentAmount = true;
                                }
                                validCoupon.Message = "Vaild coupon codes.";
                                validCoupon.StatusCode = 200;
                            }

                        }
                        else
                        {
                            validCoupon.Message = "Invaild coupon codes.";
                            validCoupon.StatusCode = 401;
                        }
                    }
                }
                else
                {
                    validCoupon.Message = "Coupon codes required.";
                    validCoupon.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                validCoupon.Message = "Error while fetching all coupon codes.";
                validCoupon.StatusCode = 500;
                Logger.Error("Get All Coupon Codes Error" + ex.StackTrace);
            }

            return validCoupon;
        }
    }
}
