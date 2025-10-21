using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFrontDisplayToBundle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FrontDisplay",
                table: "Bundles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Bundles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FrontDisplay",
                value: false);

            migrationBuilder.UpdateData(
                table: "Bundles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FrontDisplay",
                value: false);

            migrationBuilder.UpdateData(
                table: "Bundles",
                keyColumn: "Id",
                keyValue: 3,
                column: "FrontDisplay",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontDisplay",
                table: "Bundles");
        }
    }
}
