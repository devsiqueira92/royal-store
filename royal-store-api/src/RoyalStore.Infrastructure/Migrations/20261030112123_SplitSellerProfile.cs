using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId",
                table: "Profiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_Cpf",
                table: "Profiles",
                columns: new[] { "TenantId", "Cpf" },
                unique: true,
                filter: "[Cpf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_Email",
                table: "Profiles",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles",
                column: "UserId",
                unique: true);

            // Data migration: every existing Seller row (always has a UserId — employees only)
            // carries its personal fields over to a new Profile row keyed by the same UserId,
            // before those columns are dropped from Sellers below. Tenant owners (Admin) have no
            // Seller row today, so none of them get a Profile row here either — their first one is
            // still created lazily on first self-service save, per the feature's original design.
            migrationBuilder.Sql(@"
                INSERT INTO [Profiles] ([Id], [TenantId], [UserId], [FullName], [Phone], [Cpf], [Email], [CompletedAt], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy])
                SELECT NEWID(), [TenantId], [UserId], [FullName], [Phone], [Cpf], [Email], [OnboardingCompletedAt], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]
                FROM [Sellers]
                WHERE [UserId] IS NOT NULL;
            ");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_TenantId_Cpf",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_TenantId_Email",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Sellers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Sellers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Sellers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Sellers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OnboardingCompletedAt",
                table: "Sellers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Sellers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Data migration (reverse): copy each Profile's fields back onto its matching Seller
            // row by UserId, before Profiles is dropped below.
            migrationBuilder.Sql(@"
                UPDATE s
                SET s.[FullName] = p.[FullName],
                    s.[Phone] = p.[Phone],
                    s.[Cpf] = p.[Cpf],
                    s.[Email] = p.[Email],
                    s.[OnboardingCompletedAt] = p.[CompletedAt]
                FROM [Sellers] s
                INNER JOIN [Profiles] p ON p.[UserId] = s.[UserId];
            ");

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

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
