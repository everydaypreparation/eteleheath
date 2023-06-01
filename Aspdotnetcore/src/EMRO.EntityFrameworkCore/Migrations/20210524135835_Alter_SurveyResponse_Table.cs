using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Alter_SurveyResponse_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponse_Users_UniqueUserId",
                table: "SurveyResponse");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponse_UniqueUserId",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "UniqueUserId",
                table: "SurveyResponse");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UniqueUserId",
                table: "SurveyResponse",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_UniqueUserId",
                table: "SurveyResponse",
                column: "UniqueUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponse_Users_UniqueUserId",
                table: "SurveyResponse",
                column: "UniqueUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
