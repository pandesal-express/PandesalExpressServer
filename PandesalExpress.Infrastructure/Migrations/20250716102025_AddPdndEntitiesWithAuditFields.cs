using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PandesalExpress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPdndEntitiesWithAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pdnd_requests",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(26)", nullable: false),
                    store_id = table.Column<string>(type: "char(26)", nullable: false),
                    requesting_employee_id = table.Column<string>(type: "char(26)", nullable: false),
                    commissary_id = table.Column<string>(type: "char(26)", nullable: true),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_needed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    commissary_notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    status_last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_updated_by = table.Column<string>(type: "char(26)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pdnd_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_pdnd_requests_AspNetUsers_commissary_id",
                        column: x => x.commissary_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pdnd_requests_AspNetUsers_last_updated_by",
                        column: x => x.last_updated_by,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pdnd_requests_AspNetUsers_requesting_employee_id",
                        column: x => x.requesting_employee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pdnd_requests_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pdnd_request_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(26)", nullable: false),
                    pdnd_request_id = table.Column<string>(type: "char(26)", nullable: false),
                    product_id = table.Column<string>(type: "char(26)", nullable: false),
                    product_name = table.Column<string>(type: "varchar(180)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pdnd_request_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_pdnd_request_items_pdnd_requests_pdnd_request_id",
                        column: x => x.pdnd_request_id,
                        principalTable: "pdnd_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pdnd_request_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_request_items_pdnd_request_id",
                table: "pdnd_request_items",
                column: "pdnd_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_request_items_product_id",
                table: "pdnd_request_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_requests_commissary_id",
                table: "pdnd_requests",
                column: "commissary_id");

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_requests_last_updated_by",
                table: "pdnd_requests",
                column: "last_updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_requests_requesting_employee_id",
                table: "pdnd_requests",
                column: "requesting_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_pdnd_requests_store_id",
                table: "pdnd_requests",
                column: "store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pdnd_request_items");

            migrationBuilder.DropTable(
                name: "pdnd_requests");
        }
    }
}
