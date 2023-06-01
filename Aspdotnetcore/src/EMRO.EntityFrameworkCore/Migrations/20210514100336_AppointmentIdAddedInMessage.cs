using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class AppointmentIdAddedInMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "UserMessages",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_AppointmentId",
                table: "UserMessages",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_DoctorAppointment_AppointmentId",
                table: "UserMessages",
                column: "AppointmentId",
                principalTable: "DoctorAppointment",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_DoctorAppointment_AppointmentId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_AppointmentId",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "UserMessages");
        }
    }
}
