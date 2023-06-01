using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Add_SurveyResponse_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurveyResponse",
                columns: table => new
                {
                    ResponseId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    QuestionId = table.Column<Guid>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    ResponseTime = table.Column<DateTime>(nullable: true),
                    AppointmentId = table.Column<Guid>(nullable: true),
                    UniqueUserId = table.Column<long>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    CaseId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponse", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_SurveyResponse_SurveyQuestionMaster_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "SurveyQuestionMaster",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponse_Users_UniqueUserId",
                        column: x => x.UniqueUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_QuestionId",
                table: "SurveyResponse",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_UniqueUserId",
                table: "SurveyResponse",
                column: "UniqueUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyResponse");
        }
    }
}
