using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class kk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodsReturnOrderItems_ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderItems_ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems",
                column: "ReceiptPurchaseOrderItemId",
                unique: true,
                filter: "[ReceiptPurchaseOrderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches",
                column: "ReceiptPurchaseOrderBatchId",
                unique: true,
                filter: "[ReceiptPurchaseOrderBatchId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodsReturnOrderItems_ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderItems_ReceiptPurchaseOrderItemId",
                table: "GoodsReturnOrderItems",
                column: "ReceiptPurchaseOrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReturnOrderBatches_ReceiptPurchaseOrderBatchId",
                table: "GoodsReturnOrderBatches",
                column: "ReceiptPurchaseOrderBatchId",
                unique: true);
        }
    }
}
