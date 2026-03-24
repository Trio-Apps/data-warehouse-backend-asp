using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class newFieldInStockTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceivingStatus",
                table: "TransferredStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "TransferredStockBatches",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "TransferredItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivingStatus",
                table: "TransferredStocks");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "TransferredStockBatches");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "TransferredItems");
        }
    }
}
