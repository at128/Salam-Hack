using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SalamHack.Infrastructure.Data;

#nullable disable

namespace SalamHack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260429120000_AddExpenseSoftDelete")]
    public partial class AddExpenseSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                table: "expenses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_DeletedAtUtc",
                table: "expenses",
                column: "DeletedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_expenses_DeletedAtUtc",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "expenses");
        }
    }
}
