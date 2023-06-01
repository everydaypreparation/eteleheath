using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Meeting_Statistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MissedAppointment",
                table: "DoctorAppointment",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MeetingLogDetails",
                columns: table => new
                {
                    MeetingLogId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    AppointmentId = table.Column<Guid>(nullable: false),
                    MeetingId = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingLogDetails", x => x.MeetingLogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingLogDetails");

            migrationBuilder.DropColumn(
                name: "MissedAppointment",
                table: "DoctorAppointment");
        }
    }
}
