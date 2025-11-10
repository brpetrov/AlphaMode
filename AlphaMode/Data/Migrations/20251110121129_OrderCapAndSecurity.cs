using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderCapAndSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerKey",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Orders",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerKey_CreatedUtc",
                table: "Orders",
                columns: new[] { "CustomerKey", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeviceId_CreatedUtc",
                table: "Orders",
                columns: new[] { "DeviceId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IpAddress_CreatedUtc",
                table: "Orders",
                columns: new[] { "IpAddress", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerKey_CreatedUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeviceId_CreatedUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IpAddress_CreatedUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerKey",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Orders");
        }
    }
}
