using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Add2columnsinrequesttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "RequestConsultants",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "RequestConsultants",
                type: "varchar(50)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "RequestConsultants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "RequestConsultants");
        }
    }
}
