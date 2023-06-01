using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addcontconfigurationchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseRate",
                table: "CostConfigurations");

            migrationBuilder.DropColumn(
                name: "ConsultationFee",
                table: "CostConfigurations");

            migrationBuilder.DropColumn(
                name: "PerPageRate",
                table: "CostConfigurations");

            migrationBuilder.DropColumn(
                name: "UptoPages",
                table: "CostConfigurations");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "CostConfigurations",
                newName: "KeyName");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "CostConfigurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "CostConfigurations");

            migrationBuilder.RenameColumn(
                name: "KeyName",
                table: "CostConfigurations",
                newName: "RoleName");

            migrationBuilder.AddColumn<double>(
                name: "BaseRate",
                table: "CostConfigurations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ConsultationFee",
                table: "CostConfigurations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PerPageRate",
                table: "CostConfigurations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "UptoPages",
                table: "CostConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
