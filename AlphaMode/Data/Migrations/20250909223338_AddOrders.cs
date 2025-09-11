using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedUtc",
                table: "Orders",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedUtc",
                table: "Orders",
                columns: new[] { "Status", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreatedUtc",
                table: "Orders");
        }
    }
}
