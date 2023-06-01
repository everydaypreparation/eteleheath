using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class repmoveuserconsentfamilydocotordetailstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_UserConsentFamilyDoctorsDetails_UserConsentFam~",
                table: "UserConsents");

            migrationBuilder.DropTable(
                name: "UserConsentFamilyDoctorsDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserConsents_UserConsentFamilyDoctorsDetailsId",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "UserConsentFamilyDoctorsDetailsId",
                table: "UserConsents");

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "InviteUser",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "InviteUser",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "InviteUser");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "InviteUser");

            migrationBuilder.AddColumn<Guid>(
                name: "UserConsentFamilyDoctorsDetailsId",
                table: "UserConsents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserConsentFamilyDoctorsDetails",
                columns: table => new
                {
                    UserConsentFamilyDoctorsDetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "varchar(512)", nullable: true),
                    City = table.Column<string>(type: "varchar(50)", nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EmailID = table.Column<string>(type: "varchar(250)", nullable: true),
                    FirstName = table.Column<string>(type: "varchar(250)", nullable: true),
                    Gender = table.Column<string>(type: "varchar(20)", nullable: true),
                    LastName = table.Column<string>(type: "varchar(250)", nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    State = table.Column<string>(type: "varchar(50)", nullable: true),
                    TelePhone = table.Column<string>(type: "varchar(50)", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UniqueUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserConsentPatientsDetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsentFamilyDoctorsDetails", x => x.UserConsentFamilyDoctorsDetailsId);
                    table.ForeignKey(
                        name: "FK_UserConsentFamilyDoctorsDetails_Users_UniqueUserId",
                        column: x => x.UniqueUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserConsentFamilyDoctorsDetails_UserConsentPatientsDetails_~",
                        column: x => x.UserConsentPatientsDetailsId,
                        principalTable: "UserConsentPatientsDetails",
                        principalColumn: "UserConsentPatientsDetailsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserConsentFamilyDoctorsDetailsId",
                table: "UserConsents",
                column: "UserConsentFamilyDoctorsDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsentFamilyDoctorsDetails_UniqueUserId",
                table: "UserConsentFamilyDoctorsDetails",
                column: "UniqueUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsentFamilyDoctorsDetails_UserConsentPatientsDetailsId",
                table: "UserConsentFamilyDoctorsDetails",
                column: "UserConsentPatientsDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_UserConsentFamilyDoctorsDetails_UserConsentFam~",
                table: "UserConsents",
                column: "UserConsentFamilyDoctorsDetailsId",
                principalTable: "UserConsentFamilyDoctorsDetails",
                principalColumn: "UserConsentFamilyDoctorsDetailsId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
