using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "ReceiptPurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNum",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPurchaseOrderItems_PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems",
                column: "PurchaseOrderItemId",
                unique: true,
                filter: "[PurchaseOrderItemId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptPurchaseOrderItems_PurchaseOrderItems_PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems",
                column: "PurchaseOrderItemId",
                principalTable: "PurchaseOrderItems",
                principalColumn: "PurchaseOrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptPurchaseOrderItems_PurchaseOrderItems_PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptPurchaseOrderItems_PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "ReceiptPurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderItemId",
                table: "ReceiptPurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "LineNum",
                table: "PurchaseOrderItems");
        }
    }
}
