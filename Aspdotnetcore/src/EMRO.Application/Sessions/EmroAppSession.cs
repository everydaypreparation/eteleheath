using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace EMRO.Sessions
{

    public class EmroAppSession : ClaimsAbpSession, ITransientDependency
    {
        public EmroAppSession(
        IPrincipalAccessor principalAccessor,
        IMultiTenancyConfig multiTenancy,
        ITenantResolver tenantResolver,
        IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider) :
        base(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider)
        {

        }
        public string UniqueUserId
        {
            get
            {
                var userEmailClaim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == "Application_UniqueUserId");
                if (string.IsNullOrEmpty(userEmailClaim?.Value))
                {
                    return null;
                }

                return userEmailClaim.Value;
            }
        }
    }
}
