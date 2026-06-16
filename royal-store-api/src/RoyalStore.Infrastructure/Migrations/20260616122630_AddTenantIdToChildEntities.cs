using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToChildEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SaleItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CashMovements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_TenantId",
                table: "SalePayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_TenantId",
                table: "SaleItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_TenantId",
                table: "CashMovements",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalePayments_TenantId",
                table: "SalePayments");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_TenantId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_CashMovements_TenantId",
                table: "CashMovements");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CashMovements");
        }
    }
}
