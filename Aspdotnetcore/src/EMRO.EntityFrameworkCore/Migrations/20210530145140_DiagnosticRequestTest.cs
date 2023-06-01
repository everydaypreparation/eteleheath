using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class DiagnosticRequestTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestTests",
                columns: table => new
                {
                    RequestTestId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    PatientId = table.Column<Guid>(nullable: false),
                    DiagnosticId = table.Column<Guid>(nullable: true),
                    ConsultantId = table.Column<Guid>(nullable: false),
                    ReportType = table.Column<string>(nullable: true),
                    ReportDetails = table.Column<string>(nullable: true),
                    DueDate = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTests", x => x.RequestTestId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestTests");
        }
    }
}
