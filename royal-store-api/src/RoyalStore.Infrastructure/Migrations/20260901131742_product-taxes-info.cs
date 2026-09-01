using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class producttaxesinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "DefaultCfop",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IcmsCst",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelectiveTax",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IssCode",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IvaCst",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nbs",
                table: "Products",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PisCofinsCst",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Height",
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

            migrationBuilder.DropColumn(
                name: "DefaultCfop",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IcmsCst",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsSelectiveTax",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IssCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IvaCst",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Nbs",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PisCofinsCst",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Products");
        }
    }
}
