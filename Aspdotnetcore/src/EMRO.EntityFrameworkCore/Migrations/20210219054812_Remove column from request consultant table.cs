using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Removecolumnfromrequestconsultanttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityDate",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "AvailabilityEndTime",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "AvailabilityStartTime",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "SlotZoneEndTime",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "SlotZoneTime",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "RequestConsultants");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityDate",
                table: "RequestConsultants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityEndTime",
                table: "RequestConsultants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStartTime",
                table: "RequestConsultants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlotZoneEndTime",
                table: "RequestConsultants",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SlotZoneTime",
                table: "RequestConsultants",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "RequestConsultants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "RequestConsultants",
                type: "text",
                nullable: true);
        }
    }
}
