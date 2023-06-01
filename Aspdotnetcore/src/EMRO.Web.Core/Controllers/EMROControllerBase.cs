using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace EMRO.Controllers
{
    public abstract class EMROControllerBase: AbpController
    {
        protected EMROControllerBase()
        {
            LocalizationSourceName = EMROConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
