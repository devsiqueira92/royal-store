using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCostCompositionIntoLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HAND-EDITED on purpose (064 FR-023, research D3).
            //
            // The generated migration renamed "ProductCostItems" to "CostCompositionItems" and swapped its
            // ProductId for a list id. That would have turned every line saved straight on a product by
            // spec 063 into a row pointing at a list that does not exist.
            //
            // The old table is therefore left exactly as it is: it is no longer part of the model, nothing
            // reads or writes it, and its rows are not deleted here. A later clean-up migration drops it.
            // The model snapshot is the model's, so "migrations has-pending-model-changes" stays clean.

            migrationBuilder.CreateTable(
                name: "CostCompositionLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCompositionLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCompositionLists_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CostCompositionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCompositionListId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_CostCompositionItems", x => x.Id);
                    table.CheckConstraint("CK_CostCompositionItems_Value_NonNegative", "\"Value\" >= 0");
                    table.ForeignKey(
                        name: "FK_CostCompositionItems_CostCompositionLists_CostCompositionLi~",
                        column: x => x.CostCompositionListId,
                        principalTable: "CostCompositionLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostCompositionItems_CostCompositionListId",
                table: "CostCompositionItems",
                column: "CostCompositionListId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCompositionItems_TenantId",
                table: "CostCompositionItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCompositionLists_ProductId",
                table: "CostCompositionLists",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostCompositionLists_TenantId",
                table: "CostCompositionLists",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only the two tables this migration created. "ProductCostItems" was never touched by Up,
            // so it is not touched here either.
            migrationBuilder.DropTable(
                name: "CostCompositionItems");

            migrationBuilder.DropTable(
                name: "CostCompositionLists");
        }
    }
}
