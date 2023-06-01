using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class ConsultantReportIntakeForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultantReportsIds",
                table: "UserConsentPatientsDetails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipWithPatient",
                table: "UserConsentPatientsDetails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeFirstName",
                table: "UserConsentPatientsDetails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeLastName",
                table: "UserConsentPatientsDetails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultantReportsIds",
                table: "UserConsentPatientsDetails");

            migrationBuilder.DropColumn(
                name: "RelationshipWithPatient",
                table: "UserConsentPatientsDetails");

            migrationBuilder.DropColumn(
                name: "RepresentativeFirstName",
                table: "UserConsentPatientsDetails");

            migrationBuilder.DropColumn(
                name: "RepresentativeLastName",
                table: "UserConsentPatientsDetails");
        }
    }
}
