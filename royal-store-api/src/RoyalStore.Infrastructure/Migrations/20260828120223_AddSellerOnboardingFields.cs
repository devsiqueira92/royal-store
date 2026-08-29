using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sellers_TenantId_Cpf",
                table: "Sellers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Sellers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Sellers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Sellers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OnboardingCompletedAt",
                table: "Sellers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_TenantId_Cpf",
                table: "Sellers",
                columns: new[] { "TenantId", "Cpf" },
                unique: true,
                filter: "[Cpf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_TenantId_Email",
                table: "Sellers",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            // Backfill: every existing Seller-role account gets a Sellers profile row
            // with no CPF/email/phone and no OnboardingCompletedAt, so those users are
            // routed through the onboarding gate on their next sign-in. Audit columns
            // are set explicitly because raw SQL bypasses the audit interceptor.
            migrationBuilder.Sql(@"
INSERT INTO Sellers
    (Id, TenantId, FullName, Phone, Cpf, Email, IsActive, UserId,
     OnboardingCompletedAt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT NEWID(), u.TenantId, u.UserName, NULL, NULL, NULL, 1, u.Id,
       NULL, SYSDATETIMEOFFSET(), 'system-backfill', SYSDATETIMEOFFSET(), 'system-backfill'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r ON r.Id = ur.RoleId AND r.NormalizedName = 'SELLER'
WHERE NOT EXISTS (SELECT 1 FROM Sellers s WHERE s.UserId = u.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: backfilled Sellers rows (CreatedBy = 'system-backfill') are intentionally
            // left in place — they cannot be reliably told apart from later genuine rows.
            migrationBuilder.DropIndex(
                name: "IX_Sellers_TenantId_Cpf",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_TenantId_Email",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "Sellers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Sellers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Sellers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_TenantId_Cpf",
                table: "Sellers",
                columns: new[] { "TenantId", "Cpf" },
                unique: true);
        }
    }
}
