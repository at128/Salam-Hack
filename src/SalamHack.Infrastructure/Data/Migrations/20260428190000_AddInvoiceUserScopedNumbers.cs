using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SalamHack.Infrastructure.Data;

#nullable disable

namespace SalamHack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260428190000_AddInvoiceUserScopedNumbers")]
    public partial class AddInvoiceUserScopedNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_customers_CustomerId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_InvoiceNumber",
                table: "invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE i
                SET UserId = p.UserId
                FROM invoices AS i
                INNER JOIN projects AS p
                    ON p.Id = i.ProjectId
                    AND p.CustomerId = i.CustomerId
                WHERE i.UserId IS NULL
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "invoices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_CustomerId_UserId",
                table: "invoices",
                columns: new[] { "CustomerId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_UserId",
                table: "invoices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_UserId_InvoiceNumber",
                table: "invoices",
                columns: new[] { "UserId", "InvoiceNumber" },
                unique: true,
                filter: "[DeletedAtUtc] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_AspNetUsers_UserId",
                table: "invoices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_customers_CustomerId_UserId",
                table: "invoices",
                columns: new[] { "CustomerId", "UserId" },
                principalTable: "customers",
                principalColumns: new[] { "Id", "UserId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_AspNetUsers_UserId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_customers_CustomerId_UserId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_CustomerId_UserId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_UserId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_UserId_InvoiceNumber",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceNumber",
                table: "invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_customers_CustomerId",
                table: "invoices",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
