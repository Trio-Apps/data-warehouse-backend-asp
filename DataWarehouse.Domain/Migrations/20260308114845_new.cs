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
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "TransferredStocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "TransferredRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "QuantityAdjustmentStocks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "TransferredStocks");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "TransferredRequests");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "QuantityAdjustmentStocks");
        }
    }
}
