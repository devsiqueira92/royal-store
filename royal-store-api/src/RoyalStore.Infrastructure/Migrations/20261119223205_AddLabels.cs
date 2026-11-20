using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabelFormats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaperType = table.Column<int>(type: "integer", nullable: false),
                    PaperWidth = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    PaperHeight = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    LabelWidth = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    LabelHeight = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    HorizontalSpacing = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    VerticalSpacing = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MarginTop = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MarginRight = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MarginBottom = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MarginLeft = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelFormats", x => x.Id);
                    table.CheckConstraint("CK_LabelFormats_Columns_Range", "\"Columns\" BETWEEN 1 AND 20");
                    table.CheckConstraint("CK_LabelFormats_Offsets_NonNegative", "\"HorizontalSpacing\" >= 0 AND \"VerticalSpacing\" >= 0 AND \"MarginTop\" >= 0 AND \"MarginRight\" >= 0 AND \"MarginBottom\" >= 0 AND \"MarginLeft\" >= 0");
                    table.CheckConstraint("CK_LabelFormats_Sizes_Positive", "\"PaperWidth\" > 0 AND \"PaperHeight\" > 0 AND \"LabelWidth\" > 0 AND \"LabelHeight\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "LabelTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LabelFormatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    SourceTemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelTemplates", x => x.Id);
                    table.CheckConstraint("CK_LabelTemplates_Default_Active", "NOT \"IsDefault\" OR \"IsActive\"");
                    table.ForeignKey(
                        name: "FK_LabelTemplates_LabelFormats_LabelFormatId",
                        column: x => x.LabelFormatId,
                        principalTable: "LabelFormats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabelFormats_TenantId",
                table: "LabelFormats",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_LabelFormatId",
                table: "LabelTemplates",
                column: "LabelFormatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_TenantId_Default",
                table: "LabelTemplates",
                column: "TenantId",
                unique: true,
                filter: "\"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_TenantId_IsActive",
                table: "LabelTemplates",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_TenantId_NormalizedName",
                table: "LabelTemplates",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelTemplates");

            migrationBuilder.DropTable(
                name: "LabelFormats");
        }
    }
}
