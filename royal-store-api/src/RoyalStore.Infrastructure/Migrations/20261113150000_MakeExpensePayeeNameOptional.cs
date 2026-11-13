using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <summary>
    /// Makes the payee optional on expenses and on recurring templates: plenty of operating costs
    /// have no payee worth naming, and an invented one is worse than none.
    /// </summary>
    /// <remarks>
    /// Widening a NOT NULL column to nullable rewrites no rows, so this applies instantly on a
    /// populated table. <c>Down</c> cannot say the same: it has to put something in the rows that
    /// hold NULL before the constraint goes back, which is why it writes a placeholder rather than
    /// failing — see the comment there.
    /// </remarks>
    public partial class MakeExpensePayeeNameOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayeeName",
                table: "Expenses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PayeeName",
                table: "ExpenseRecurrences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A plain AlterColumn back to NOT NULL fails on any row left without a payee, which is
            // every row this feature was added for. The rows are filled with an empty string first
            // so the rollback completes — the information is gone either way, and a failed
            // rollback that leaves the schema half-migrated is worse.
            migrationBuilder.Sql(
                "UPDATE \"Expenses\" SET \"PayeeName\" = '' WHERE \"PayeeName\" IS NULL;");

            migrationBuilder.Sql(
                "UPDATE \"ExpenseRecurrences\" SET \"PayeeName\" = '' WHERE \"PayeeName\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "PayeeName",
                table: "Expenses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PayeeName",
                table: "ExpenseRecurrences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
