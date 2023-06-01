using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addinviteuserforignkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InviteUserId",
                table: "UserConsents",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_InviteUserId",
                table: "UserConsents",
                column: "InviteUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_InviteUser_InviteUserId",
                table: "UserConsents",
                column: "InviteUserId",
                principalTable: "InviteUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_InviteUser_InviteUserId",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_UserConsents_InviteUserId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "InviteUserId",
                table: "UserConsents");
        }
    }
}
