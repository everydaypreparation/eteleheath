using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class removecolumnformusermetadetailstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorAddress",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorCity",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorCountry",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorEmailID",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorFirstName",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorLastName",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorPostalCodes",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorState",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "DoctorTelePhone",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "ConsentMedicalInformationWithCancerCareProvider",
                table: "InviteUser");

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyDoctorId",
                table: "UserMetaDetails",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InviteUserId",
                table: "UserMetaDetails",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InviteUser",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FamilyDoctorId",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "InviteUserId",
                table: "UserMetaDetails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InviteUser");

            migrationBuilder.AddColumn<string>(
                name: "DoctorAddress",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorCity",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorCountry",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorEmailID",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorFirstName",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorLastName",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorPostalCodes",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorState",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorTelePhone",
                table: "UserMetaDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentMedicalInformationWithCancerCareProvider",
                table: "InviteUser",
                type: "text",
                nullable: true);
        }
    }
}
