using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using EMRO.EntityFrameworkCore;
using EMRO.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace EMRO.Web.Tests
{
    [DependsOn(
        typeof(EMROWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class EMROWebTestModule : AbpModule
    {
        public EMROWebTestModule(EMROEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(EMROWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(EMROWebMvcModule).Assembly);
        }
    }
}