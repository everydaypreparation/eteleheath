using EMRO.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AppointmentIdInDBFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.BuguuidAppointmentApiFunctionFunctions_20200108053432();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
