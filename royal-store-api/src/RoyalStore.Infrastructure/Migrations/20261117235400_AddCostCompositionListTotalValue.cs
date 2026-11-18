using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostCompositionListTotalValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                table: "CostCompositionLists",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Lists that already exist get the sum of their lines; from here on the create and update
            // handlers keep it current. Data only — the schema change is the column above.
            migrationBuilder.Sql(
                """
                UPDATE "CostCompositionLists" AS l
                SET "TotalValue" = COALESCE(
                    (SELECT SUM(i."Value") FROM "CostCompositionItems" AS i WHERE i."CostCompositionListId" = l."Id"),
                    0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "CostCompositionLists");
        }
    }
}
