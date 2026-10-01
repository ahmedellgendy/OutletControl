    using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutletControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReceipts_OutletId",
                table: "StockReceipts");

            migrationBuilder.DropIndex(
                name: "IX_Sales_OutletId",
                table: "Sales");

            migrationBuilder.AddColumn<string>(
                name: "ClientReferenceId",
                table: "StockReceipts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientReferenceId",
                table: "Sales",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReceipts_OutletId_ClientReferenceId",
                table: "StockReceipts",
                columns: new[] { "OutletId", "ClientReferenceId" },
                unique: true,
                filter: "[ClientReferenceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OutletId_ClientReferenceId",
                table: "Sales",
                columns: new[] { "OutletId", "ClientReferenceId" },
                unique: true,
                filter: "[ClientReferenceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReceipts_OutletId_ClientReferenceId",
                table: "StockReceipts");

            migrationBuilder.DropIndex(
                name: "IX_Sales_OutletId_ClientReferenceId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ClientReferenceId",
                table: "StockReceipts");

            migrationBuilder.DropColumn(
                name: "ClientReferenceId",
                table: "Sales");

            migrationBuilder.CreateIndex(
                name: "IX_StockReceipts_OutletId",
                table: "StockReceipts",
                column: "OutletId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OutletId",
                table: "Sales",
                column: "OutletId");
        }
    }
}
