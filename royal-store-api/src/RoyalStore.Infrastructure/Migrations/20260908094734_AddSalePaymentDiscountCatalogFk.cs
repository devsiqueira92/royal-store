using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalePaymentDiscountCatalogFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaymentDiscountApplied",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "SalePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_PaymentId",
                table: "SalePayments",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalePayments_Payments_PaymentId",
                table: "SalePayments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalePayments_Payments_PaymentId",
                table: "SalePayments");

            migrationBuilder.DropIndex(
                name: "IX_SalePayments_PaymentId",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "PaymentDiscountApplied",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "SalePayments");
        }
    }
}
