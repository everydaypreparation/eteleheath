using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Remove_UserCol_AuditEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AuditEvent");

            migrationBuilder.DropColumn(
                name: "UserTitle",
                table: "AuditEvent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AuditEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserTitle",
                table: "AuditEvent",
                type: "text",
                nullable: true);
        }
    }
}
