using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class addamountdepositcolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountDeposit",
                table: "UserMetaDetails",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountDeposit",
                table: "UserMetaDetails");
        }
    }
}
