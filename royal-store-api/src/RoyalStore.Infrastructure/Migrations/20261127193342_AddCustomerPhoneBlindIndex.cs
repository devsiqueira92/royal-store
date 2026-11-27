using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPhoneBlindIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneHash",
                table: "Customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_PhoneHash",
                table: "Customers",
                columns: new[] { "TenantId", "PhoneHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_PhoneHash",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneHash",
                table: "Customers");
        }
    }
}
