using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PandesalExpress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Store_Inventory_Composite_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_store_inventories_store_id",
                table: "store_inventories");

            migrationBuilder.CreateIndex(
                name: "IX_store_inventories_store_id_product_id",
                table: "store_inventories",
                columns: new[] { "store_id", "product_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_store_inventories_store_id_product_id",
                table: "store_inventories");

            migrationBuilder.CreateIndex(
                name: "IX_store_inventories_store_id",
                table: "store_inventories",
                column: "store_id");
        }
    }
}
