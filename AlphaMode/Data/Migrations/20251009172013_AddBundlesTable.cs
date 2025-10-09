using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlphaMode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBundlesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Orders",
                newName: "BundleId");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Bundles",
                columns: new[] { "Id", "IsActive", "Name", "Price", "Size" },
                values: new object[,]
                {
                    { 1, true, "1 опаковка", 55m, 1 },
                    { 2, true, "2 опаковки", 100m, 2 },
                    { 3, true, "3 опаковки", 140m, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BundleId",
                table: "Orders",
                column: "BundleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Bundles_BundleId",
                table: "Orders",
                column: "BundleId",
                principalTable: "Bundles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Bundles_BundleId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Bundles");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BundleId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "BundleId",
                table: "Orders",
                newName: "Quantity");
        }
    }
}
