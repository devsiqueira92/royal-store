using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLabelConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LabelFormats",
                columns: new[] { "Id", "Columns", "CreatedAt", "CreatedBy", "HorizontalSpacing", "IsActive", "IsSystem", "LabelHeight", "LabelWidth", "MarginBottom", "MarginLeft", "MarginRight", "MarginTop", "Name", "PaperHeight", "PaperType", "PaperWidth", "TenantId", "UpdatedAt", "UpdatedBy", "VerticalSpacing" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0066-0000000000f1"), 3, new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 2.5m, true, true, 38.1m, 63.5m, 15.15m, 7.25m, 7.25m, 15.15m, "A4 — 3 × 7 (63,5 × 38,1 mm)", 297m, 0, 210m, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 0m },
                    { new Guid("00000000-0000-0000-0066-0000000000f2"), 6, new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 2m, true, true, 18m, 33m, 12.7m, 4m, 4m, 12.7m, "Carta — 6 colunas (33 × 18 mm)", 279m, 0, 216m, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 0m },
                    { new Guid("00000000-0000-0000-0066-0000000000f3"), 3, new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 2m, true, true, 21.3m, 33m, 2m, 2m, 2m, 2m, "Rolo térmico 3 colunas (33 × 21,3 mm)", 300m, 1, 107m, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", 2m }
                });

            migrationBuilder.InsertData(
                table: "LabelTemplates",
                columns: new[] { "Id", "Configuration", "CreatedAt", "CreatedBy", "IsActive", "LabelFormatId", "Name", "NormalizedName", "SourceTemplateId", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0066-0000000000a1"), "{\"version\":1,\"elements\":[{\"type\":\"text\",\"field\":\"product.name\",\"x\":1,\"y\":1.5,\"width\":61,\"height\":6.5,\"fontSize\":11,\"fontWeight\":\"bold\",\"alignment\":\"left\"},{\"type\":\"text\",\"field\":\"product.price\",\"x\":1,\"y\":14.5,\"width\":60.5,\"height\":7.5,\"fontSize\":14,\"fontWeight\":\"bold\",\"alignment\":\"center\",\"format\":\"currency\"},{\"type\":\"barcode\",\"field\":\"product.barcode\",\"x\":2.5,\"y\":25.5,\"width\":59.5,\"height\":11,\"format\":\"CODE128\",\"showValue\":true},{\"type\":\"text\",\"field\":\"product.sku\",\"x\":1,\"y\":8,\"width\":22,\"height\":5.5,\"fontSize\":6,\"fontWeight\":\"normal\",\"alignment\":\"left\"}]}", new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", true, new Guid("00000000-0000-0000-0066-0000000000f1"), "Gôndola A4 (3 colunas)", "GÔNDOLA A4 (3 COLUNAS)", null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
                    { new Guid("00000000-0000-0000-0066-0000000000a2"), "{\"version\":1,\"elements\":[{\"type\":\"text\",\"field\":\"product.name\",\"x\":1,\"y\":0,\"width\":32,\"height\":5.5,\"fontSize\":6,\"fontWeight\":\"bold\",\"alignment\":\"left\"},{\"type\":\"text\",\"field\":\"product.price\",\"x\":1,\"y\":6,\"width\":32,\"height\":4,\"fontSize\":8,\"fontWeight\":\"bold\",\"alignment\":\"center\",\"format\":\"currency\"},{\"type\":\"barcode\",\"field\":\"product.barcode\",\"x\":2,\"y\":10,\"width\":30,\"height\":7.5,\"format\":\"CODE128\",\"showValue\":true}]}", new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", true, new Guid("00000000-0000-0000-0066-0000000000f2"), "Gôndola Carta (6 colunas)", "GÔNDOLA CARTA (6 COLUNAS)", null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
                    { new Guid("00000000-0000-0000-0066-0000000000a3"), "{\"version\":1,\"elements\":[{\"type\":\"text\",\"field\":\"product.name\",\"x\":1,\"y\":0,\"width\":30.5,\"height\":6.5,\"fontSize\":6,\"fontWeight\":\"bold\",\"alignment\":\"left\"},{\"type\":\"text\",\"field\":\"product.price\",\"x\":1,\"y\":6.5,\"width\":16,\"height\":5.5,\"fontSize\":8,\"fontWeight\":\"bold\",\"alignment\":\"center\",\"format\":\"currency\",\"showLabel\":false},{\"type\":\"barcode\",\"field\":\"product.sku\",\"x\":0.5,\"y\":14,\"width\":32,\"height\":6.5,\"fontSize\":4.5,\"format\":\"CODE128\",\"showValue\":true}]}", new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", true, new Guid("00000000-0000-0000-0066-0000000000f3"), "Rolo térmico 3 colunas (33 × 21,3)", "ROLO TÉRMICO 3 COLUNAS (33 × 21,3)", null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(2026, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_LabelTemplates_System_NotDefault",
                table: "LabelTemplates",
                sql: "NOT (\"TenantId\" = '00000000-0000-0000-0000-000000000000' AND \"IsDefault\")");

            // Rows stored before 066 may carry IsSystem = true under a store; a system format is
            // exactly one owned by no store, so align the flag before the constraint checks it.
            migrationBuilder.Sql(
                "UPDATE \"LabelFormats\" SET \"IsSystem\" = (\"TenantId\" = '00000000-0000-0000-0000-000000000000') " +
                "WHERE \"IsSystem\" <> (\"TenantId\" = '00000000-0000-0000-0000-000000000000');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LabelFormats_System_Tenant",
                table: "LabelFormats",
                sql: "\"IsSystem\" = (\"TenantId\" = '00000000-0000-0000-0000-000000000000')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LabelTemplates_System_NotDefault",
                table: "LabelTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LabelFormats_System_Tenant",
                table: "LabelFormats");

            migrationBuilder.DeleteData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000a1"));

            migrationBuilder.DeleteData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000a2"));

            migrationBuilder.DeleteData(
                table: "LabelTemplates",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000a3"));

            migrationBuilder.DeleteData(
                table: "LabelFormats",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000f1"));

            migrationBuilder.DeleteData(
                table: "LabelFormats",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000f2"));

            migrationBuilder.DeleteData(
                table: "LabelFormats",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0066-0000000000f3"));
        }
    }
}
