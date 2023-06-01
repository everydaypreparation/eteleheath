using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addcronhistortable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CronHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    AppointmentId = table.Column<Guid>(nullable: false),
                    ScheduleTime = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronHistories", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CronHistories");
        }
    }
}
