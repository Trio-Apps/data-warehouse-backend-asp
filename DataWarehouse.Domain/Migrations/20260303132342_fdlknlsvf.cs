using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataWarehouse.Domain.Migrations
{
    /// <inheritdoc />
    public partial class fdlknlsvf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "SalesReturnOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNum",
                table: "SalesReturnOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "SalesReturnOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "SalesReturnOrders");

            migrationBuilder.DropColumn(
                name: "DocNum",
                table: "SalesReturnOrders");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "SalesReturnOrders");
        }
    }
}
