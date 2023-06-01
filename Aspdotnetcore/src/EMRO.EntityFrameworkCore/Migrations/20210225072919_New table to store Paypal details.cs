using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class NewtabletostorePaypaldetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAppointmentPayPal",
                columns: table => new
                {
                    PayId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    PayeeEmailAddress = table.Column<string>(nullable: true),
                    PayeeMerchantId = table.Column<string>(nullable: true),
                    PayerFullName = table.Column<string>(nullable: true),
                    PayerEmailAddress = table.Column<string>(nullable: true),
                    PayerId = table.Column<int>(nullable: false),
                    OriginalPayAmount = table.Column<double>(nullable: true),
                    PayAmount = table.Column<double>(nullable: true),
                    CouponId = table.Column<Guid>(nullable: true),
                    AppointmentId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    PaymentCurrencyCode = table.Column<string>(nullable: true),
                    PaymentCreateTime = table.Column<DateTime>(nullable: false),
                    PaymentUpdateTime = table.Column<DateTime>(nullable: false),
                    PaymentOrderId = table.Column<DateTime>(nullable: false),
                    PaymentStatus = table.Column<string>(nullable: true),
                    PayerAddressLine = table.Column<string>(nullable: true),
                    PayerAdminArea2 = table.Column<string>(nullable: true),
                    PayerAdminArea1 = table.Column<string>(nullable: true),
                    PayerPostalCode = table.Column<string>(nullable: true),
                    PayerCountryCode = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppointmentPayPal", x => x.PayId);
                    table.ForeignKey(
                        name: "FK_UserAppointmentPayPal_CouponMaster_CouponId",
                        column: x => x.CouponId,
                        principalTable: "CouponMaster",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAppointmentPayPal_CouponId",
                table: "UserAppointmentPayPal",
                column: "CouponId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAppointmentPayPal");
        }
    }
}
