using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AddCostConfigurationtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostConfigurations",
                columns: table => new
                {
                    CostId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    RoleName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ConsultationFee = table.Column<double>(nullable: false),
                    BaseRate = table.Column<double>(nullable: false),
                    PerPageRate = table.Column<double>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostConfigurations", x => x.CostId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostConfigurations");
        }
    }
}
