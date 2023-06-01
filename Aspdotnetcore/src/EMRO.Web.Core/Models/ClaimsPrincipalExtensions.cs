using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;

namespace EMRO.Models
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return claim?.Value;
        }

        public static bool IsImpersonating(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var isImpersonating = principal.HasClaim("IsImpersonating", "true");

            return isImpersonating;
        }
    }
}
