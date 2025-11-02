using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryMethod",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryMethod",
                table: "Orders",
                column: "DeliveryMethod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "Orders");
        }
    }
}
