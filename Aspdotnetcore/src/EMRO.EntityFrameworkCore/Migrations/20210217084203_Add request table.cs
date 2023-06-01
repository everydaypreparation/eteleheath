using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addrequesttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestConsultants",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(type: "varchar(250)", nullable: true),
                    LastName = table.Column<string>(type: "varchar(250)", nullable: true),
                    Specialty = table.Column<string>(nullable: true),
                    Hospital = table.Column<string>(nullable: true),
                    UniqueUserId = table.Column<long>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    AvailabilityDate = table.Column<string>(nullable: true),
                    AvailabilityStartTime = table.Column<string>(nullable: true),
                    AvailabilityEndTime = table.Column<string>(nullable: true),
                    TimeZone = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    SlotZoneTime = table.Column<DateTime>(nullable: false),
                    SlotZoneEndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestConsultants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestConsultants_Users_UniqueUserId",
                        column: x => x.UniqueUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestConsultants_UniqueUserId",
                table: "RequestConsultants",
                column: "UniqueUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestConsultants");
        }
    }
}
