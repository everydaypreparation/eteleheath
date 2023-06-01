using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AddreponsecolumninUserconsentformtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobSequenceNumber",
                table: "UserConsentForms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateRequestId",
                table: "UserConsentForms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeySha256",
                table: "UserConsentForms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionScope",
                table: "UserConsentForms",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlobStorage",
                table: "UserConsentForms",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VersionId",
                table: "UserConsentForms",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobSequenceNumber",
                table: "UserConsentForms");

            migrationBuilder.DropColumn(
                name: "CreateRequestId",
                table: "UserConsentForms");

            migrationBuilder.DropColumn(
                name: "EncryptionKeySha256",
                table: "UserConsentForms");

            migrationBuilder.DropColumn(
                name: "EncryptionScope",
                table: "UserConsentForms");

            migrationBuilder.DropColumn(
                name: "IsBlobStorage",
                table: "UserConsentForms");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "UserConsentForms");
        }
    }
}
