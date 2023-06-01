using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AuditEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvent",
                columns: table => new
                {
                    AuditEventId = table.Column<Guid>(nullable: false),
                    ExecutionTime = table.Column<DateTime>(nullable: false),
                    ImpersonatorUserId = table.Column<long>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    BrowserInfo = table.Column<string>(nullable: true),
                    ClientName = table.Column<string>(nullable: true),
                    ClientIpAddress = table.Column<string>(nullable: true),
                    ExecutionDuration = table.Column<int>(nullable: false),
                    ReturnValue = table.Column<string>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    ServiceName = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: true),
                    ImpersonatorTenantId = table.Column<int>(nullable: true),
                    Parameters = table.Column<string>(nullable: true),
                    CustomData = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    UniqueUserId = table.Column<Guid>(nullable: false),
                    UserTitle = table.Column<string>(nullable: true),
                    Operation = table.Column<string>(nullable: true),
                    Component = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    BeforeValue = table.Column<string>(nullable: true),
                    AfterValue = table.Column<string>(nullable: true),
                    IsImpersonating = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvent", x => x.AuditEventId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvent");
        }
    }
}
