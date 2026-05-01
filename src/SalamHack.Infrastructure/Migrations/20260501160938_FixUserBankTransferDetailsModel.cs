using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalamHack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserBankTransferDetailsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIban",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BankIban",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "AspNetUsers");
        }
    }
}
