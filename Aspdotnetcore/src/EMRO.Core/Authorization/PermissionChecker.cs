using Abp.Authorization;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;

namespace EMRO.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
