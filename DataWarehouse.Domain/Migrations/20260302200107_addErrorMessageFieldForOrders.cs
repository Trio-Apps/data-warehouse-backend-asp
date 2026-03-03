using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addErrorMessageFieldForOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "SalesReturnOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "SalesOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ReceiptPurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "GoodsReturnOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "DeliveryNoteOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "SalesReturnOrders");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "GoodsReturnOrders");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "DeliveryNoteOrders");
        }
    }
}
