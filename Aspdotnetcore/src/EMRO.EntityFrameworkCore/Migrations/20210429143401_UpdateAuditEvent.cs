using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class UpdateAuditEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomData",
                table: "AuditEvent");

            migrationBuilder.DropColumn(
                name: "Exception",
                table: "AuditEvent");

            migrationBuilder.DropColumn(
                name: "MethodName",
                table: "AuditEvent");

            migrationBuilder.DropColumn(
                name: "ReturnValue",
                table: "AuditEvent");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "AuditEvent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomData",
                table: "AuditEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "AuditEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MethodName",
                table: "AuditEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnValue",
                table: "AuditEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "AuditEvent",
                type: "text",
                nullable: true);
        }
    }
}
