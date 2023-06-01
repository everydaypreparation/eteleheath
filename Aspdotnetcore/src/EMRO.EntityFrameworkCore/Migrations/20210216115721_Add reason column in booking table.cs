using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addreasoncolumninbookingtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "DoctorAppointment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "DoctorAppointment");
        }
    }
}
