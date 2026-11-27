using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalStore.Infrastructure.Migrations
{
    /// <summary>
    /// Widens PII columns to <c>text</c> for ciphertext and adds the blind-index (<c>…Hash</c>)
    /// columns. Schema only: existing rows stay plaintext until <c>PiiEncryptionBackfillSeeder</c>
    /// encrypts them and fills the hashes on the next API start (it needs the keys, SQL can't).
    /// <para>Down is only safe before that backfill has run — afterwards the ciphertext no longer
    /// fits the old varchar/date columns. Restore from backup instead.</para>
    /// </summary>
    public partial class EncryptPiiAtRest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Document",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TenantId_Cnpj",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TenantId_Cpf",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TenantId_Email",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_Document",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Tenants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "Tenants",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CnpjHash",
                table: "Suppliers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressStreet",
                table: "Sales",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressNumber",
                table: "Sales",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfHash",
                table: "Profiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailHash",
                table: "Profiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BirthDate",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "Customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Complement",
                table: "CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInfo",
                table: "CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DocumentHash",
                table: "Tenants",
                column: "DocumentHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TenantId_CnpjHash",
                table: "Suppliers",
                columns: new[] { "TenantId", "CnpjHash" },
                unique: true,
                filter: "\"CnpjHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_CpfHash",
                table: "Profiles",
                columns: new[] { "TenantId", "CpfHash" },
                unique: true,
                filter: "\"CpfHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_EmailHash",
                table: "Profiles",
                columns: new[] { "TenantId", "EmailHash" },
                unique: true,
                filter: "\"EmailHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_DocumentHash",
                table: "Customers",
                columns: new[] { "TenantId", "DocumentHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_DocumentHash",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TenantId_CnpjHash",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TenantId_CpfHash",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TenantId_EmailHash",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_DocumentHash",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CnpjHash",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CpfHash",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "EmailHash",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Tenants",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Suppliers",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressStreet",
                table: "Sales",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressNumber",
                table: "Sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Profiles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Profiles",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Customers",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Customers",
                type: "date",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "CustomerAddresses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CustomerAddresses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Complement",
                table: "CustomerAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInfo",
                table: "CustomerAddresses",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Document",
                table: "Tenants",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TenantId_Cnpj",
                table: "Suppliers",
                columns: new[] { "TenantId", "Cnpj" },
                unique: true,
                filter: "\"Cnpj\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_Cpf",
                table: "Profiles",
                columns: new[] { "TenantId", "Cpf" },
                unique: true,
                filter: "\"Cpf\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_Email",
                table: "Profiles",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Document",
                table: "Customers",
                columns: new[] { "TenantId", "Document" },
                unique: true);
        }
    }
}
