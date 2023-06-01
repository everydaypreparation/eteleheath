using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Add_Column_SurveyResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SurveyResponse",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "SurveyResponse",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "SurveyResponse",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "SurveyResponse",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SurveyResponse",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "SurveyResponse",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "SurveyResponse",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "SurveyResponse");
        }
    }
}
