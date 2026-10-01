using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutletControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBalanceRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StockBalances",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Outlets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Outlets",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Outlets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Outlets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Outlets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Outlets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CompanyName", "CurrencyCode", "LogoUrl", "Phone", "TaxNumber" },
                values: new object[] { "Friday Ice Cream", "EGP", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StockBalances");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Outlets");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Outlets");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Outlets");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Outlets");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Outlets");
        }
    }
}
