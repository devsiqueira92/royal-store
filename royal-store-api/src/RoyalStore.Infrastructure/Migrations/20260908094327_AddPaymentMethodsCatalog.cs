using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodsCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentType = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_PaymentType",
                table: "Payments",
                columns: new[] { "TenantId", "PaymentType" },
                unique: true);

            // Backfill for tenants that already existed before this feature (research.md Decision 12):
            // every current tenant gets the same 9 default payment methods new tenants get
            // automatically going forward (RegisterTenantCommandHandler / Payment.SeedDefaults).
            migrationBuilder.Sql(@"
                INSERT INTO [Payments] ([Id], [TenantId], [PaymentMethod], [PaymentType], [Discount], [Active], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy])
                SELECT NEWID(), [Id], 'Cash', '01', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Credit Card', '03', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Debit Card', '04', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Store Card', '05', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Gift Card', '12', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Pix (Dynamic)', '17', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Cashback / Loyalty Credit', '19', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Pix (Static)', '20', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants]
                UNION ALL
                SELECT NEWID(), [Id], 'Store Credit', '21', 0, 1, SYSDATETIMEOFFSET(), 'system-migration', SYSDATETIMEOFFSET(), 'system-migration' FROM [Tenants];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
