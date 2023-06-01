using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EMRO.EntityFrameworkCore
{
    public static class EMRODbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<EMRODbContext> builder, string connectionString)
        {
            builder.UseNpgsql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<EMRODbContext> builder, DbConnection connection)
        {
            builder.UseNpgsql(connection);
        }
    }
}
