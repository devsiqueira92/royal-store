using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceListPercentageRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BasePriceListId",
                table: "PriceLists",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "PriceLists",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_BasePriceListId",
                table: "PriceLists",
                column: "BasePriceListId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PriceLists_Rule_Complete",
                table: "PriceLists",
                sql: "(\"DiscountPercentage\" IS NULL) = (\"BasePriceListId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PriceLists_Rule_NotSelf",
                table: "PriceLists",
                sql: "\"BasePriceListId\" <> \"Id\"");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceLists_PriceLists_BasePriceListId",
                table: "PriceLists",
                column: "BasePriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceLists_PriceLists_BasePriceListId",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_PriceLists_BasePriceListId",
                table: "PriceLists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PriceLists_Rule_Complete",
                table: "PriceLists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PriceLists_Rule_NotSelf",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "BasePriceListId",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "PriceLists");
        }
    }
}
