using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addcoulumninconsultreporttabl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBlobStorage",
                table: "OncologyConsultReports",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReportRequestId",
                table: "OncologyConsultReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureRequestId",
                table: "OncologyConsultReports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlobStorage",
                table: "OncologyConsultReports");

            migrationBuilder.DropColumn(
                name: "ReportRequestId",
                table: "OncologyConsultReports");

            migrationBuilder.DropColumn(
                name: "SignatureRequestId",
                table: "OncologyConsultReports");
        }
    }
}
