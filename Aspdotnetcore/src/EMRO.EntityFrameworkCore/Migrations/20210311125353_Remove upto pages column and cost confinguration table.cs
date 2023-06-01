using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Removeuptopagescolumnandcostconfingurationtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UptoPages",
                table: "CostConfigurations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UptoPages",
                table: "CostConfigurations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
