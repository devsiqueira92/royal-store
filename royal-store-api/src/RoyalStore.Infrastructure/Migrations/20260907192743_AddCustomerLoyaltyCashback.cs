using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerLoyaltyCashback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashbackApplied",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashbackEarned",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoyaltyTierId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CashbackEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbackEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clawbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clawbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CashbackPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CashbackValidityDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashbackRedemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashbackEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbackRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashbackRedemptions_CashbackEntries_CashbackEntryId",
                        column: x => x.CashbackEntryId,
                        principalTable: "CashbackEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LoyaltyTierId",
                table: "Customers",
                column: "LoyaltyTierId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbackEntries_TenantId_CustomerId_State_ExpiresAt",
                table: "CashbackEntries",
                columns: new[] { "TenantId", "CustomerId", "State", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CashbackRedemptions_CashbackEntryId",
                table: "CashbackRedemptions",
                column: "CashbackEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbackRedemptions_TenantId_CashbackEntryId",
                table: "CashbackRedemptions",
                columns: new[] { "TenantId", "CashbackEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashbackRedemptions_TenantId_SaleId",
                table: "CashbackRedemptions",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Clawbacks_TenantId_SaleId",
                table: "Clawbacks",
                columns: new[] { "TenantId", "SaleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTiers_TenantId_Name",
                table: "LoyaltyTiers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            // Backfill for tenants that already existed before this feature (research.md Decision 9):
            // every current tenant gets the four seeded tiers new tenants get automatically going
            // forward (RegisterTenantCommandHandler), and every existing customer is pointed at their
            // tenant's Standard tier — the same default a newly-created customer resolves to (FR-013).
            migrationBuilder.Sql(@"
                INSERT INTO [LoyaltyTiers] ([Id], [TenantId], [Name], [CashbackPercentage], [CashbackValidityDays], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy])
                SELECT NEWID(), [Id], 'VIP', 10.0, 90, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Gold', 7.0, 90, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Silver', 4.0, 90, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Standard', 2.0, 90, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants];
            ");

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.[LoyaltyTierId] = t.[Id]
                FROM [Customers] c
                INNER JOIN [LoyaltyTiers] t ON t.[TenantId] = c.[TenantId] AND t.[Name] = 'Standard';
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_LoyaltyTiers_LoyaltyTierId",
                table: "Customers",
                column: "LoyaltyTierId",
                principalTable: "LoyaltyTiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_LoyaltyTiers_LoyaltyTierId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "CashbackRedemptions");

            migrationBuilder.DropTable(
                name: "Clawbacks");

            migrationBuilder.DropTable(
                name: "LoyaltyTiers");

            migrationBuilder.DropTable(
                name: "CashbackEntries");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Customers_LoyaltyTierId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CashbackApplied",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CashbackEarned",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LoyaltyTierId",
                table: "Customers");
        }
    }
}
