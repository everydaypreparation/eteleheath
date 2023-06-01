using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Add_SurveyQuestionMaster_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurveyQuestionMaster",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    QuestionText = table.Column<string>(nullable: true),
                    QuestionType = table.Column<string>(type: "varchar(512)", nullable: true),
                    OptionSet = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestionMaster", x => x.QuestionId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyQuestionMaster");
        }
    }
}
