using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using EMRO.Authorization;

namespace EMRO
{
    [DependsOn(
        typeof(EMROCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class EMROApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<EMROAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(EMROApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
