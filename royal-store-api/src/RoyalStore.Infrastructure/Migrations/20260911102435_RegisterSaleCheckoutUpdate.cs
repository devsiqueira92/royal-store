using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RegisterSaleCheckoutUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountMode",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "AuthCode",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "Nsu",
                table: "SalePayments");

            // Change/AdditionalFees/Method are dropped outright rather than renamed onto
            // TaxAmount/LoyaltyDiscountApplied/FiscalType: the auto-generated diff proposed renames
            // because the CLR types happen to coincide, but the *meaning* is unrelated — renaming
            // would silently reinterpret every historical sale's change-given amount as a tax amount
            // and its old 4-value PaymentMethod code as a completely different FiscalPaymentType code
            // (e.g. old Method=1/CreditCard would read back as FiscalType=1/Cash). How pre-existing
            // sales should be backfilled is out of this feature's scope (spec.md Assumptions); new
            // columns default to 0/Other(99) — an honest "not reliably known" rather than a fabricated
            // value — for rows recorded before this migration.
            migrationBuilder.DropColumn(
                name: "Change",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "AdditionalFees",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "SalePayments");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyDiscountApplied",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FiscalType",
                table: "SalePayments",
                type: "int",
                nullable: false,
                defaultValue: 99); // FiscalPaymentType.Other — legacy rows predate this classification

            migrationBuilder.RenameColumn(
                name: "CardBrand",
                table: "SalePayments",
                newName: "CardFlag");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentDiscountApplied",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CashShiftId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "BranchId",
                table: "Sales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashbackRemaining",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomerLoyaltyTierName",
                table: "Sales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Sales",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentMode",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "SalePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CashbackRemaining",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerLoyaltyTierName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "FulfillmentMode",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "LoyaltyDiscountApplied",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "FiscalType",
                table: "SalePayments");

            migrationBuilder.AddColumn<decimal>(
                name: "Change",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalFees",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "SalePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn(
                name: "CardFlag",
                table: "SalePayments",
                newName: "CardBrand");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentDiscountApplied",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "CashShiftId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountMode",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "SalePayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AuthCode",
                table: "SalePayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nsu",
                table: "SalePayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
