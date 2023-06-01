using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addcolumnsinattachmenttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobSequenceNumber",
                table: "UserMessagesAttachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateRequestId",
                table: "UserMessagesAttachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeySha256",
                table: "UserMessagesAttachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionScope",
                table: "UserMessagesAttachments",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlobStorage",
                table: "UserMessagesAttachments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VersionId",
                table: "UserMessagesAttachments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobSequenceNumber",
                table: "UserMessagesAttachments");

            migrationBuilder.DropColumn(
                name: "CreateRequestId",
                table: "UserMessagesAttachments");

            migrationBuilder.DropColumn(
                name: "EncryptionKeySha256",
                table: "UserMessagesAttachments");

            migrationBuilder.DropColumn(
                name: "EncryptionScope",
                table: "UserMessagesAttachments");

            migrationBuilder.DropColumn(
                name: "IsBlobStorage",
                table: "UserMessagesAttachments");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "UserMessagesAttachments");
        }
    }
}
