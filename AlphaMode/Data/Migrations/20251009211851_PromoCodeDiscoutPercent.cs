using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromoCodeDiscoutPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "PromoCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Bundles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 59m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "PromoCodes");

            migrationBuilder.UpdateData(
                table: "Bundles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 55m);
        }
    }
}
