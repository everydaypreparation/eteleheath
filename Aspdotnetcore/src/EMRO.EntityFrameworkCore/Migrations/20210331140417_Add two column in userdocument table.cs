using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addtwocolumninuserdocumenttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreateRequestId",
                table: "UserDocuments",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlobStorage",
                table: "UserDocuments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateRequestId",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "IsBlobStorage",
                table: "UserDocuments");
        }
    }
}
