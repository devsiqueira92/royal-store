using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <summary>
    /// Cost compositions become reusable: the link moves from the list (CostCompositionLists.ProductId,
    /// unique) to the product (Products.CostCompositionId), so many products can use the same list.
    /// Existing links are carried over before the old column goes.
    /// </summary>
    public partial class ReusableCostCompositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCompositionId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Products" p
                SET "CostCompositionId" = l."Id"
                FROM "CostCompositionLists" l
                WHERE l."ProductId" = p."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CostCompositionLists_Products_ProductId",
                table: "CostCompositionLists");

            migrationBuilder.DropIndex(
                name: "IX_CostCompositionLists_ProductId",
                table: "CostCompositionLists");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "CostCompositionLists");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "CostCompositionLists");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CostCompositionId",
                table: "Products",
                column: "CostCompositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CostCompositionLists_CostCompositionId",
                table: "Products",
                column: "CostCompositionId",
                principalTable: "CostCompositionLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_CostCompositionLists_CostCompositionId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CostCompositionId",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "CostCompositionLists",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "CostCompositionLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // One product per list again: a shared list keeps only one of its products (the lowest id).
            migrationBuilder.Sql("""
                UPDATE "CostCompositionLists" l
                SET "ProductId" = (
                    SELECT MIN(p."Id"::text)::uuid FROM "Products" p
                    WHERE p."CostCompositionId" = l."Id");
                """);

            migrationBuilder.DropColumn(
                name: "CostCompositionId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_CostCompositionLists_ProductId",
                table: "CostCompositionLists",
                column: "ProductId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CostCompositionLists_Products_ProductId",
                table: "CostCompositionLists",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
