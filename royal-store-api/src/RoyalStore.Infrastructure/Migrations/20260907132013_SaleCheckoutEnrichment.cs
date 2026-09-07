using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SaleCheckoutEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalFees",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Change",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountMode",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AuthCode",
                table: "SalePayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "SalePayments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Installments",
                table: "SalePayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nsu",
                table: "SalePayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "SalePayments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ncm",
                table: "SaleItems",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "Cfop",
                table: "SaleItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountItem",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FederalRate",
                table: "SaleItems",
                type: "decimal(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IcmsCst",
                table: "SaleItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportedFederalRate",
                table: "SaleItems",
                type: "decimal(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelectiveTax",
                table: "SaleItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IssCode",
                table: "SaleItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IvaCst",
                table: "SaleItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MunicipalRate",
                table: "SaleItems",
                type: "decimal(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NationalRate",
                table: "SaleItems",
                type: "decimal(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nbs",
                table: "SaleItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionName",
                table: "SaleItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "SaleItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PisCofinsCst",
                table: "SaleItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "SaleItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StateRate",
                table: "SaleItems",
                type: "decimal(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitMeasure",
                table: "SaleItems",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VariantColor",
                table: "SaleItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "SaleItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "VariantLabel",
                table: "SaleItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VariantSize",
                table: "SaleItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Document = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TermsAccepted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_VariantId",
                table: "SaleItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Document",
                table: "Customers",
                columns: new[] { "TenantId", "Document" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_ProductVariants_VariantId",
                table: "SaleItems");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_VariantId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "AdditionalFees",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Change",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DiscountMode",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "AuthCode",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "Installments",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "Nsu",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "Cfop",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "DiscountItem",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "FederalRate",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "IcmsCst",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "ImportedFederalRate",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "IsSelectiveTax",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "IssCode",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "IvaCst",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "MunicipalRate",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "NationalRate",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Nbs",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "OptionName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "PisCofinsCst",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "StateRate",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "UnitMeasure",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "VariantColor",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "VariantLabel",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "VariantSize",
                table: "SaleItems");

            migrationBuilder.AlterColumn<string>(
                name: "Ncm",
                table: "SaleItems",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);
        }
    }
}
