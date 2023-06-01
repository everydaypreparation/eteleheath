using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using EMRO.Authorization.Users;
using EMRO.Identity;
using EMRO.Models;
using Abp.Runtime.Security;
using System.IdentityModel.Tokens.Jwt;
using EMRO.Authentication.JwtBearer;
using EMRO.Models.TokenAuth;
using Abp.Authorization;

namespace EMRO.Controllers
{
    [AbpAuthorize]
    [Route("api/v{version:apiversion}/[controller]/[action]")]
    public class ImpersonationController : EMROControllerBase
    {
        private readonly UserManager _userManager;
        private readonly SignInManager _signInManager;
        private readonly TokenAuthConfiguration _configuration;

        public ImpersonationController(
            UserManager userManager,
            TokenAuthConfiguration configuration,
            SignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<AuthenticateResultModel> ImpersonateUser(string userId)
        {

            if (User.IsInRole("Admin") == true)
            {
                var currentUserId = User.GetUserId();

                var impersonatedUser = await _userManager.FindByIdAsync(userId);
                var userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);

                userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", currentUserId));
                userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));
                userPrincipal.Identities.First().AddClaim(new Claim(AbpClaimTypes.ImpersonatorUserId, currentUserId));

                if (AbpSession.TenantId != null)
                {
                    userPrincipal.Identities.First().AddClaim(new Claim(AbpClaimTypes.ImpersonatorTenantId, AbpSession.TenantId.ToString()));
                }

                // sign out the current user
                await _signInManager.SignOutAsync();

                int timeinsecond = 7200;

                ClaimsIdentity claimsIdentity = new ClaimsIdentity();

                foreach (ClaimsIdentity identity in userPrincipal.Identities)
                {
                    claimsIdentity = identity;
                }

                var accessToken = CreateAccessToken(CreateJwtClaims(claimsIdentity, impersonatedUser), TimeSpan.FromSeconds(timeinsecond));

                // sign in the impersonate user
                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal);

                return new AuthenticateResultModel
                {
                    AccessToken = accessToken,
                    EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                    ExpireInSeconds = timeinsecond,
                    UserId = impersonatedUser.UniqueUserId,
                    StatusCode = 200,
                    Message = "Impersonation started successfully.."
                };
            }
            else
            {
                Logger.Error("You are not authorized to access this API.");
                return new AuthenticateResultModel
                {
                    StatusCode = 401,
                    Message = "You are not authorized to access this API."
                };
            }
        }

        [HttpGet]
        public async Task<AuthenticateResultModel> StopImpersonation(bool rememberClient = false)
        {
            if (!User.IsImpersonating())
            {
                Logger.Error("You are not impersonating now. Can't stop impersonation");
                return new AuthenticateResultModel
                {
                    StatusCode = 401,
                    Message = "You are not impersonating now. Can't stop impersonation"
                };
            }

            var originalUserId = User.FindFirst("OriginalUserId").Value;
            var originalUser = await _userManager.FindByIdAsync(originalUserId);

            await _signInManager.SignOutAsync();

            await _signInManager.SignInAsync(originalUser, isPersistent: true);

            var userPrincipal = await _signInManager.CreateUserPrincipalAsync(originalUser);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();

            foreach (ClaimsIdentity identity in userPrincipal.Identities)
            {
                claimsIdentity = identity;
            }

            int timeinsecond = rememberClient == false ? 7200 : 86400; ;
            var accessToken = CreateAccessToken(CreateJwtClaims(claimsIdentity, originalUser), TimeSpan.FromSeconds(timeinsecond));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = timeinsecond,
                UserId = originalUser.UniqueUserId,
                StatusCode = 200,
                Message = "Impersonation ended successfully."
            };
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

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}
