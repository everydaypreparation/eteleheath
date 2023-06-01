using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addblobresponsecolumninuserdocumenttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobSequenceNumber",
                table: "UserDocuments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeySha256",
                table: "UserDocuments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionScope",
                table: "UserDocuments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionId",
                table: "UserDocuments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobSequenceNumber",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "EncryptionKeySha256",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "EncryptionScope",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "UserDocuments");
        }
    }
}
