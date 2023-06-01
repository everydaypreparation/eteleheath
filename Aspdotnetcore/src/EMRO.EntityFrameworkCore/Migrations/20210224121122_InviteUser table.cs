using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class InviteUsertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InviteUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(type: "varchar(250)", nullable: true),
                    LastName = table.Column<string>(type: "varchar(250)", nullable: true),
                    Address = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    PostalCodes = table.Column<string>(type: "varchar(20)", nullable: true),
                    TelePhone = table.Column<string>(type: "varchar(20)", nullable: true),
                    EmailAddress = table.Column<string>(type: "varchar(250)", nullable: true),
                    ConsentMedicalInformationWithCancerCareProvider = table.Column<string>(nullable: true),
                    Hospital = table.Column<string>(nullable: true),
                    UserType = table.Column<string>(nullable: true),
                    UniqueUserId = table.Column<long>(nullable: true),
                    ReferedBy = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    OnboardedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    IsCompleted = table.Column<bool>(nullable: false),
                    IsOnboarded = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteUser_Users_UniqueUserId",
                        column: x => x.UniqueUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InviteUser_UniqueUserId",
                table: "InviteUser",
                column: "UniqueUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteUser");
        }
    }
}
