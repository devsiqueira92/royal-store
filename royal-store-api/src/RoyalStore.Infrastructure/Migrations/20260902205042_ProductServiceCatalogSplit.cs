using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductServiceCatalogSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------
            // 1. New table first — everything below needs somewhere to seed into.
            // ---------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "TaxProfileTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetItemType = table.Column<int>(type: "int", nullable: false),
                    DefaultCfop = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IcmsCst = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    PisCofinsCst = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IvaCst = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IsSelectiveTax = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxProfileTemplates", x => x.Id);
                });

            // ---------------------------------------------------------------
            // 2. New / relaxed columns. All additive; existing rows get the
            //    declared default and are corrected by the backfill in step 4.
            // ---------------------------------------------------------------
            migrationBuilder.AlterColumn<decimal>(
                name: "StockQuantity",
                table: "ProductVariants",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductVariants",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "OptionName",
                table: "ProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSelectiveTax",
                table: "Products",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.RenameColumn(
                name: "ReferenceCode",
                table: "Products",
                newName: "Sku");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCostPrice",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "Products",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "Products",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "Products",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "Products",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "Products",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OverrideTaxProfile",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StockTrack",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxProfileTemplateId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            // ---------------------------------------------------------------
            // 3. Seed one Product-kind and one Service-kind default template
            //    per existing tenant (research.md Decision 4 / spec Clarifications:
            //    every product/service must end up referencing a template).
            // ---------------------------------------------------------------
            migrationBuilder.Sql(@"
INSERT INTO TaxProfileTemplates
    (Id, TenantId, Name, Description, TargetItemType, IsSelectiveTax, IsActive, IsDefault,
     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    NEWID(), t.Id, N'Default Product Tax Profile',
    N'Auto-provisioned by the 033-product-service-catalog-split migration for pre-existing products.',
    0, 0, 1, 1,
    SYSDATETIMEOFFSET(), N'system-migration', SYSDATETIMEOFFSET(), N'system-migration'
FROM Tenants t;

INSERT INTO TaxProfileTemplates
    (Id, TenantId, Name, Description, TargetItemType, IsSelectiveTax, IsActive, IsDefault,
     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    NEWID(), t.Id, N'Default Service Tax Profile',
    N'Auto-provisioned by the 033-product-service-catalog-split migration; no pre-existing services referenced it.',
    1, 0, 1, 1,
    SYSDATETIMEOFFSET(), N'system-migration', SYSDATETIMEOFFSET(), N'system-migration'
FROM Tenants t;
");

            // ---------------------------------------------------------------
            // 4. Backfill existing Products rows before the source columns
            //    (ProductVariants.IsStockTracked etc.) are dropped below.
            //    Every pre-existing row is ItemType.Product (0) — Service was
            //    introduced in feature 032 and has no production usage yet.
            // ---------------------------------------------------------------
            migrationBuilder.Sql(@"
UPDATE p
SET p.TaxProfileTemplateId = tpt.Id
FROM Products p
INNER JOIN TaxProfileTemplates tpt
    ON tpt.TenantId = p.TenantId AND tpt.TargetItemType = 0;

UPDATE p
SET
    p.BasePrice = COALESCE((SELECT MIN(v.Price) FROM ProductVariants v WHERE v.ProductId = p.Id), 0),
    p.BaseCostPrice = COALESCE((SELECT MIN(v.CostPrice) FROM ProductVariants v WHERE v.ProductId = p.Id), 0)
FROM Products p;

UPDATE p
SET p.StockTrack = CASE
    WHEN EXISTS (SELECT 1 FROM ProductVariants v WHERE v.ProductId = p.Id AND v.IsStockTracked = 1)
    THEN 1 ELSE 0 END
FROM Products p;

UPDATE Products
SET Ncm = N'00000000'
WHERE Ncm IS NULL OR LTRIM(RTRIM(Ncm)) = N'';
");

            // ---------------------------------------------------------------
            // 5. Now safe to drop the retired columns/table.
            // ---------------------------------------------------------------
            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "IsStockTracked",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "PromotionalPrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "ProductTags");

            // ---------------------------------------------------------------
            // 6. Indexes and the FK, now that every row has a valid TaxProfileTemplateId
            //    and a unique Sku.
            // ---------------------------------------------------------------
            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxProfileTemplateId",
                table: "Products",
                column: "TaxProfileTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileTemplates_TenantId_TargetItemType",
                table: "TaxProfileTemplates",
                columns: new[] { "TenantId", "TargetItemType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_TaxProfileTemplates_TaxProfileTemplateId",
                table: "Products",
                column: "TaxProfileTemplateId",
                principalTable: "TaxProfileTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_TaxProfileTemplates_TaxProfileTemplateId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "TaxProfileTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Products_TaxProfileTemplateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OptionName",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "BaseCostPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OverrideTaxProfile",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockTrack",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxProfileTemplateId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "Products",
                newName: "ReferenceCode");

            migrationBuilder.AlterColumn<decimal>(
                name: "StockQuantity",
                table: "ProductVariants",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductVariants",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "ProductVariants",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "ProductVariants",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStockTracked",
                table: "ProductVariants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "ProductVariants",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "ProductVariants",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionalPrice",
                table: "ProductVariants",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "ProductVariants",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSelectiveTax",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTags_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_CategoryId",
                table: "ProductTags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_ProductId_CategoryId",
                table: "ProductTags",
                columns: new[] { "ProductId", "CategoryId" },
                unique: true);
        }
    }
}
