using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using EMRO.Authentication.External;
using EMRO.Authentication.JwtBearer;
using EMRO.Authorization;
using EMRO.Authorization.Users;
using EMRO.Models.TokenAuth;
using EMRO.MultiTenancy;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.WebUtilities;
using EMRO.Email;
using Abp.Extensions;
using Hangfire;
using EMRO.Identity;
using System.Text.RegularExpressions;
using EMRO.Authorization.Accounts;
using EMRO.Common.Templates;

namespace EMRO.Controllers
{
    [Route("api/v{version:apiversion}/[controller]/[action]")]
    public class TokenAuthController : EMROControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly ICacheManager _cacheManager;
        private readonly UserManager _userManager;
        private readonly IMailer _mailer;
        private readonly TemplateAppService _templateAppService;

        public TokenAuthController(
         LogInManager logInManager,
         SignInManager signInManager,
          ITenantCache tenantCache,
          AbpLoginResultTypeHelper abpLoginResultTypeHelper,
          TokenAuthConfiguration configuration,
          IExternalAuthConfiguration externalAuthConfiguration,
          IExternalAuthManager externalAuthManager,
          UserRegistrationManager userRegistrationManager,
          ICacheManager cacheManager,
          UserManager userManager
            , IMailer mailer
            , TemplateAppService templateAppService
          )
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _cacheManager = cacheManager;
            _userManager = userManager;
            _mailer = mailer;
            _signInManager = signInManager;
            _templateAppService = templateAppService;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            try
            {
                var loginResult = await GetLoginResultAsync(
                    model.UserNameOrEmailAddress,
                    model.Password,
                    GetTenancyNameOrNull()
                );

                if (loginResult.Result == AbpLoginResultType.Success)
                {
                    int timeinsecond = model.RememberClient == false ? 7200 : 86400;
                    var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, loginResult.User), TimeSpan.FromSeconds(timeinsecond));
                    //await _signInManager.SignInAsync(loginResult.Identity, model.RememberClient);
                    //await _signInManager.SignOutAsync();
                    //loginResult.Identity.AddClaim(new Claim("Application_UniqueUserId", loginResult.User.UniqueUserId.ToString()));
                    return new AuthenticateResultModel
                    {
                        AccessToken = accessToken,
                        EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                        ExpireInSeconds = timeinsecond,
                        UserId = loginResult.User.UniqueUserId,
                        StatusCode = 200,
                        Message = "login successfully."
                    };
                }
                else if (loginResult.Result == AbpLoginResultType.InvalidUserNameOrEmailAddress)
                {
                    return new AuthenticateResultModel
                    {
                        StatusCode = 401,
                        Message = "Invalid UserName Or Email Address."
                    };
                }
                else if (loginResult.Result == AbpLoginResultType.InvalidPassword)
                {
                    return new AuthenticateResultModel
                    {
                        StatusCode = 401,
                        Message = "Invalid Password."
                    };
                }
                else if (loginResult.Result == AbpLoginResultType.UserIsNotActive)
                {
                    return new AuthenticateResultModel
                    {
                        StatusCode = 401,
                        Message = "User Is Not Active."
                    };
                }
                else
                {
                    return new AuthenticateResultModel
                    {
                        StatusCode = 401,
                        Message = "Please enter valid user name and password."
                    };
                }

            }
            catch (Exception ex)
            {

                Logger.Info("Authenticate :" + ex.StackTrace);
                return new AuthenticateResultModel
                {
                    StatusCode = 500,
                    Message = "An internal error occurred during your request!."
                };
            }
        }

        [HttpGet]

        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, loginResult.User));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, loginResult.User)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {

            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);
            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return loginResult;
                case AbpLoginResultType.UserIsNotActive:
                    return loginResult;
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return loginResult;
                case AbpLoginResultType.LockedOut:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity, User user)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("Application_UniqueUserId", user.UniqueUserId.ToString())
            });


            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }

        [HttpPost]
        public async Task<ForgotPasswordModelOutput> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            // ObjectResult result = null;
            ForgotPasswordModelOutput forgotPasswordModelOutput = new ForgotPasswordModelOutput();
            try
            {
                if (!string.IsNullOrEmpty(model.BaseUrl))
                {
                    var user = await _userManager.FindByNameOrEmailAsync(model.EmailAddress);
                    // string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    if (user != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);


                        var param = new Dictionary<string, string>
                        {
                           {"token", token },
                           {"email", model.EmailAddress }
                        };
                        string Url = model.BaseUrl.ToString() + "/#/reset-password?token=" + token + "&email=" + user.EmailAddress;
                        /// var callbackUrl = QueryHelpers.AddQueryString(Url, param);

                        string Name = user.Name.ToPascalCase() + " " + user.Surname.ToPascalCase();
                        string message = "We have received a Reset Password request from you." + " <br /> " + " You can reset it by clicking  <a href=\"" + Url + "\">here</a>.";
                        string body = _templateAppService.GetTemplates(Name, message,"");
                        string subject = "Reset Password";
                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, subject, body, user.EmailAddress));

                        //string body = " Hello " + "<b>" + user.Name.ToPascalCase() + " " + user.Surname.ToPascalCase() + ",</b> <br /><br /> " +
                        //    "We have received a Reset Password request from you." + " <br /> " + " You can reset it by clicking  <a href=\"" + Url + "\">here</a>."
                        //     + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                        //string subject = "Reset Password";

                        //BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, subject, body));
                        //BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, subject, body, user.EmailAddress));
                        //return Content("Email Sent Sucessfully");
                        forgotPasswordModelOutput.Message = "Your password has been sent in your email id.";
                        forgotPasswordModelOutput.StatusCode = 200;

                    }
                    else
                    {
                        forgotPasswordModelOutput.Message = "User Name or Email Id does not exists.Please sign up.";
                        forgotPasswordModelOutput.StatusCode = 401;
                        //result = new ObjectResult(new { error = "User Not Found." })
                        //{
                        //    StatusCode = 401
                        //};

                        //throw new UserFriendlyException("User not found.");

                    }
                }
                else
                {
                    forgotPasswordModelOutput.Message = "Please enter base url.";
                    forgotPasswordModelOutput.StatusCode = 401;
                    //result = new ObjectResult(new { error = "Please enter base url." })
                    //{
                    //    StatusCode = 401
                    //};
                }
            }
            catch (Exception ex)
            {
                forgotPasswordModelOutput.Message = "Something went wrong, please try again.";
                forgotPasswordModelOutput.StatusCode = 500;
                Logger.Error("Forgot Password Error" + ex.StackTrace);
            }
            return forgotPasswordModelOutput;
        }

        [HttpPost]
        public async Task<ForgotPasswordModelOutput> ResetPassword([FromBody] ResetPasswordModel model)
        {
            ForgotPasswordModelOutput forgotPasswordModelOutput = new ForgotPasswordModelOutput();
            try
            {
               if(!new Regex(AccountAppService.PasswordRegex).IsMatch(model.Password))
                {
                    forgotPasswordModelOutput.Message = "Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.";
                    forgotPasswordModelOutput.StatusCode = 401;
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var code = model.Token.Replace(" ", "+");
                        var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
                        if (result.Succeeded)
                        {
                            CurrentUnitOfWork.SaveChanges();

                            forgotPasswordModelOutput.Message = "Password has been changed successfully.";
                            forgotPasswordModelOutput.StatusCode = 200;
                        }
                        else
                        {

                            foreach (var item in result.Errors)
                            {
                                if (item.Code == "InvalidToken")
                                    forgotPasswordModelOutput.Message = "This link has expired, Please try again.";
                                else
                                    forgotPasswordModelOutput.Message += item.Description;
                            }

                            forgotPasswordModelOutput.StatusCode = 401;
                        }

                    }
                    else
                    {
                        forgotPasswordModelOutput.Message = "Email Id does not exists.Please sign up.";
                        forgotPasswordModelOutput.StatusCode = 401;
                    }
                }
               
            }
            catch (Exception ex)
            {
                forgotPasswordModelOutput.Message = "Something went wrong, please try again.";
                forgotPasswordModelOutput.StatusCode = 401;
                Logger.Error("Reset Password Error" + ex.StackTrace);
            }
            return forgotPasswordModelOutput;
        }

        //[HttpPost]
        //public async Task<AuthenticateResultModel> Logout()
        //{
        //    try
        //    {
        //        await _signInManager.SignOutAsync();
        //        return new AuthenticateResultModel
        //        {
        //            StatusCode = 200,
        //            Message = "Logout successfully."
        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Info("Logout :" + ex.StackTrace);
        //        return new AuthenticateResultModel
        //        {
        //            StatusCode = 500,
        //            Message = "An internal error occurred during your request!."
        //        };
        //    }
        //}

    }
}
