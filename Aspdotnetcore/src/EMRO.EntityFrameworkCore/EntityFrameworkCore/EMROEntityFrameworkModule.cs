using Abp.Dependency;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using EMRO.EntityFrameworkCore.Seed;

namespace EMRO.EntityFrameworkCore
{
    [DependsOn(
        typeof(EMROCoreModule), 
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class EMROEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<EMRODbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        EMRODbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        EMRODbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(EMROEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            EnsureMigrated();

            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }

        private void EnsureMigrated()
        {
            using (var migrateExecuter = IocManager.ResolveAsDisposable<MultiTenantMigrateExecuter>())
            {
                migrateExecuter.Object.Run();
            }
        }
    }
}
