using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductionComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionComponentBatches");

            migrationBuilder.DropTable(
                name: "ProductionComponentLines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionComponentLines",
                columns: table => new
                {
                    ProductionComponentLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InWhsQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IssueType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionComponentLines", x => x.ProductionComponentLineId);
                    table.ForeignKey(
                        name: "FK_ProductionComponentLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ProductionComponentLines_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionComponentLines_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "ProductionComponentBatches",
                columns: table => new
                {
                    ProductionComponentBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionComponentLineId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionComponentBatches", x => x.ProductionComponentBatchId);
                    table.ForeignKey(
                        name: "FK_ProductionComponentBatches_ProductionComponentLines_ProductionComponentLineId",
                        column: x => x.ProductionComponentLineId,
                        principalTable: "ProductionComponentLines",
                        principalColumn: "ProductionComponentLineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionComponentBatches_ProductionComponentLineId",
                table: "ProductionComponentBatches",
                column: "ProductionComponentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionComponentLines_ItemId",
                table: "ProductionComponentLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionComponentLines_ProductionOrderId",
                table: "ProductionComponentLines",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionComponentLines_WarehouseId",
                table: "ProductionComponentLines",
                column: "WarehouseId");
        }
    }
}
