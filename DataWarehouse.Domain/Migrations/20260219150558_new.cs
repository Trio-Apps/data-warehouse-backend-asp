using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceiptPurchaseOrders_PurchaseOrderId",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "ReceiptPurchaseOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_PurchaseOrderId",
                table: "ReceiptPurchaseOrders",
                column: "PurchaseOrderId",
                unique: true,
                filter: "[PurchaseOrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceiptPurchaseOrders_PurchaseOrderId",
                table: "ReceiptPurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseOrderId",
                table: "ReceiptPurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrders_PurchaseOrderId",
                table: "ReceiptPurchaseOrders",
                column: "PurchaseOrderId",
                unique: true);
        }
    }
}
