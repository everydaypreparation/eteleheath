using EMRO.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Insertspecialtyandsubspecialty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertSpecialty_20210119053432();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
