using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionHeaderBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionComponentLines",
                columns: table => new
                {
                    ProductionComponentLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InWhsQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IssueType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
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
                name: "ProductionHeaderBatches",
                columns: table => new
                {
                    ProductionHeaderBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHeaderBatches", x => x.ProductionHeaderBatchId);
                    table.ForeignKey(
                        name: "FK_ProductionHeaderBatches_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionComponentBatches",
                columns: table => new
                {
                    ProductionComponentBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionComponentLineId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeaderBatches_ProductionOrderId",
                table: "ProductionHeaderBatches",
                column: "ProductionOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionComponentBatches");

            migrationBuilder.DropTable(
                name: "ProductionHeaderBatches");

            migrationBuilder.DropTable(
                name: "ProductionComponentLines");
        }
    }
}
