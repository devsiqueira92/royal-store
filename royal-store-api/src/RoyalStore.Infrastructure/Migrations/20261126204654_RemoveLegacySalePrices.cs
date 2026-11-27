using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacySalePrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sale prices now live only in ProductPrices. Before the legacy column goes, every
            // product or service still without a price on its store's default list gets one equal
            // to its former single sale price, so nothing is left unpriced. Variant prices and
            // costs are dropped: variants are priced by their product's price lists.
            migrationBuilder.Sql("""
                INSERT INTO "ProductPrices"
                    ("Id", "TenantId", "ProductId", "PriceListId", "SalePrice", "MinPrice",
                     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
                SELECT gen_random_uuid(), p."TenantId", p."Id", pl."Id", GREATEST(p."BasePrice", 0), NULL,
                       now(), 'migration', now(), 'migration'
                FROM "Products" p
                JOIN "PriceLists" pl ON pl."TenantId" = p."TenantId" AND pl."IsDefault"
                WHERE NOT EXISTS (
                    SELECT 1 FROM "ProductPrices" pp
                    WHERE pp."ProductId" = p."Id" AND pp."PriceListId" = pl."Id");
                """);

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "ProductVariants",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ProductVariants",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // The legacy column mirrored the default-list price; restore that. Variant prices and
            // costs cannot be recovered and come back empty.
            migrationBuilder.Sql("""
                UPDATE "Products" p
                SET "BasePrice" = pp."SalePrice"
                FROM "ProductPrices" pp
                JOIN "PriceLists" pl ON pl."Id" = pp."PriceListId" AND pl."IsDefault"
                WHERE pp."ProductId" = p."Id";
                """);
        }
    }
}
