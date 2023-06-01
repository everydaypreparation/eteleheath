using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AddisSamvaadflaginnotestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSamvaad",
                table: "UserNotes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSamvaad",
                table: "UserNotes");
        }
    }
}
