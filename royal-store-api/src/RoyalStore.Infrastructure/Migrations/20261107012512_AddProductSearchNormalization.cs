using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedSku",
                table: "ProductVariants",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                computedColumnSql: "fn_normalize_search_text(\"Sku\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                computedColumnSql: "fn_normalize_search_text(\"Name\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedSku",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                computedColumnSql: "fn_normalize_search_text(\"Sku\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_NormalizedSku",
                table: "ProductVariants",
                column: "NormalizedSku");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NormalizedName",
                table: "Products",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NormalizedSku",
                table: "Products",
                column: "NormalizedSku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_NormalizedSku",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_Products_NormalizedName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_NormalizedSku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NormalizedSku",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NormalizedSku",
                table: "Products");
        }
    }
}
