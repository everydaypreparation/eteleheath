using Xunit;

namespace EMRO.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute()
        {
            //if (!EMROConsts.MultiTenancyEnabled)
            //{
            //    Skip = "MultiTenancy is disabled.";
            //}
        }
    }
}
