using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EMRO.EntityFrameworkCore
{
    public static partial class MigrationBuilderExtensions
    {
        private const string RootDirName = "EntityFrameworkCore";
        private static string datetimeDirName = string.Empty;

        /// <summary>
        /// Read SQL from file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>SQL</returns>
        private static string ReadMigrationSql(string path)
        {
            FileInfo file = new FileInfo(path);
            string script = file.OpenText().ReadToEnd();
            return script;
        }

        /// <summary>
        /// Create DB functions
        /// </summary>
        /// <param name="migrationBuilder">MigrationBuilder</param>
        public static void CreatePgCryptoFunctions_20200108053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create Extension
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "create-extension.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

            // Create functions
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "create-function.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

        }

        public static void InsertTimeZoneApiFunctionFunctions_20200108053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create Extension
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "insert-timezone.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

        }

        public static void InsertSpecialty_20210119053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create functions
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "insert-specialty.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);
        }

        public static void InsertCouponMaster_20210119053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create functions
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "couponmaster.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);
        }


        public static void BuguuidMessageApiFunctionFunctions_20200108053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create Extension
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "usp-getinbox-4.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

            // Create functions
            //path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "get-message-4.sql");
            //script = ReadMigrationSql(path);
            //migrationBuilder.Sql(script);

            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "get-sent-4.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "get-trash-4.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);

        }


        public static void BuguuidAppointmentApiFunctionFunctions_20200108053432(this MigrationBuilder migrationBuilder)
        {
            datetimeDirName = "Pgscript";

            var path = string.Empty;
            var script = string.Empty;

            // Create functions
            path = Path.Combine(AppContext.BaseDirectory, RootDirName, datetimeDirName, "get-message-5.sql");
            script = ReadMigrationSql(path);
            migrationBuilder.Sql(script);
        }

    }
}
