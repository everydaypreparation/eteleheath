using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using EMRO.Configuration;
using EMRO.Web;

namespace EMRO.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class EMRODbContextFactory : IDesignTimeDbContextFactory<EMRODbContext>
    {
        public EMRODbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EMRODbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            EMRODbContextConfigurer.Configure(builder, configuration.GetConnectionString(EMROConsts.ConnectionStringName));

            return new EMRODbContext(builder.Options);
        }
    }
}
