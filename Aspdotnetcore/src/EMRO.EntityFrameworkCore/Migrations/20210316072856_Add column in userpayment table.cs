using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Addcolumninuserpaymenttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "PayAmount",
                table: "UserAppointmentPayment",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayeeEmailAddress",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayeeMerchantId",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerAddressLine",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerAdminArea1",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerAdminArea2",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerCountryCode",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerEmailAddress",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerFullName",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerId",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerPostalCode",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentCreateTime",
                table: "UserAppointmentPayment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PaymentCurrencyCode",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "UserAppointmentPayment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentOrderId",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "UserAppointmentPayment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentUpdateTime",
                table: "UserAppointmentPayment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayeeEmailAddress",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayeeMerchantId",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerAddressLine",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerAdminArea1",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerAdminArea2",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerCountryCode",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerEmailAddress",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerFullName",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerId",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PayerPostalCode",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentCreateTime",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentCurrencyCode",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentOrderId",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "UserAppointmentPayment");

            migrationBuilder.DropColumn(
                name: "PaymentUpdateTime",
                table: "UserAppointmentPayment");

            migrationBuilder.AlterColumn<long>(
                name: "PayAmount",
                table: "UserAppointmentPayment",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
