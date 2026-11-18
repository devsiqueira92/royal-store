using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostCompositionListName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CostCompositionLists",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Lists that already exist have no name and a name is required from here on: give them a
            // placeholder the manager can rename. Names are not unique, so this cannot clash.
            migrationBuilder.Sql(
                """
                UPDATE "CostCompositionLists" SET "Name" = 'Composição de custo' WHERE "Name" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CostCompositionLists");
        }
    }
}
