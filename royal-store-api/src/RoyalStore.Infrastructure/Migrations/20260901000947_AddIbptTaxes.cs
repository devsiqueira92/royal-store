using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIbptTaxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deterministic, schema-bound normalization used by the two persisted computed
            // columns below. Mirrors RoyalStore.Application.Common.TextSearchNormalizer:
            // lowercase -> fold Portuguese diacritics -> strip everything but [a-z0-9].
            migrationBuilder.Sql(@"
CREATE FUNCTION dbo.fn_NormalizeSearchText(@input nvarchar(4000))
RETURNS nvarchar(4000)
WITH SCHEMABINDING
AS
BEGIN
    IF @input IS NULL
        RETURN N'';

    DECLARE @s nvarchar(4000) = LOWER(@input);

    SET @s = TRANSLATE(@s,
        N'áàâãäéèêëíìîïóòôõöúùûüçñ',
        N'aaaaaeeeeiiiiooooouuuucn');

    DECLARE @i int = PATINDEX(N'%[^a-z0-9]%', @s COLLATE Latin1_General_BIN2);
    WHILE @i > 0
    BEGIN
        SET @s = STUFF(@s, @i, 1, N'');
        SET @i = PATINDEX(N'%[^a-z0-9]%', @s COLLATE Latin1_General_BIN2);
    END

    RETURN @s;
END
");

            migrationBuilder.CreateTable(
                name: "IbptTaxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NationalRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    FederalRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    ImportedFederalRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    StateRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    MunicipalRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    EffectiveStart = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NormalizedCode = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true, computedColumnSql: "[dbo].[fn_NormalizeSearchText]([Code])", stored: true),
                    NormalizedText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true, computedColumnSql: "[dbo].[fn_NormalizeSearchText]([Description])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IbptTaxes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IbptTaxes_Code_EffectiveStart",
                table: "IbptTaxes",
                columns: new[] { "Code", "EffectiveStart" });

            migrationBuilder.CreateIndex(
                name: "IX_IbptTaxes_NormalizedCode",
                table: "IbptTaxes",
                column: "NormalizedCode");

            migrationBuilder.CreateIndex(
                name: "IX_IbptTaxes_NormalizedText",
                table: "IbptTaxes",
                column: "NormalizedText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IbptTaxes");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_NormalizeSearchText;");
        }
    }
}
