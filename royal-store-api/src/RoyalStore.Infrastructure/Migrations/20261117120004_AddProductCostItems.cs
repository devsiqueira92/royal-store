using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCostItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCostItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostItems", x => x.Id);
                    table.CheckConstraint("CK_ProductCostItems_Value_NonNegative", "\"Value\" >= 0");
                    table.ForeignKey(
                        name: "FK_ProductCostItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostItems_ProductId",
                table: "ProductCostItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostItems_TenantId",
                table: "ProductCostItems",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCostItems");
        }
    }
}
