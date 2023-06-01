using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class addcolumninusertabletostoreblobresponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobSequenceNumber",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateRequestId",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeySha256",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionScope",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlobStorage",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VersionId",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobSequenceNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreateRequestId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptionKeySha256",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptionScope",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBlobStorage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "Users");
        }
    }
}
