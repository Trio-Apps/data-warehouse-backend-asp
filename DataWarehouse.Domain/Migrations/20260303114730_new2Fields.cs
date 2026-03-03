using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class new2Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "TransferredStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "TransferredStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "TransferredStocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "TransferredItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "SalesReturnOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "ReceivedStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "ReceivedStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "ReceivedStocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "ReceivedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "ReceiptPurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "ReceiptPurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "ReceiptPurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "GoodsReturnOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "GoodsReturnOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "GoodsReturnOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "GoodsReturnOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "CountStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "CountStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "CountStocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "CountStockItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "TransferredStocks");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "TransferredStocks");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "TransferredStocks");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "TransferredItems");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "SalesReturnOrderItems");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "ReceivedStocks");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "ReceivedStocks");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "ReceivedStocks");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "ReceivedItems");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "GoodsReturnOrders");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "GoodsReturnOrders");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "GoodsReturnOrders");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "GoodsReturnOrderItems");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "CountStocks");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "CountStocks");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "CountStocks");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "CountStockItems");
        }
    }
}
