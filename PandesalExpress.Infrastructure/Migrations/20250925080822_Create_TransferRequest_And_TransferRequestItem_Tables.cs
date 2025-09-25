using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PandesalExpress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_TransferRequest_And_TransferRequestItem_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transfer_requests",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(26)", nullable: false),
                    sending_store_id = table.Column<string>(type: "char(26)", nullable: false),
                    receiving_store_id = table.Column<string>(type: "char(26)", nullable: false),
                    initiating_employee_id = table.Column<string>(type: "char(26)", nullable: false),
                    responding_employee_id = table.Column<string>(type: "char(26)", nullable: true),
                    status = table.Column<string>(type: "varchar(15)", nullable: false),
                    request_notes = table.Column<string>(type: "varchar(500)", nullable: true),
                    response_notes = table.Column<string>(type: "varchar(500)", nullable: true),
                    system_message = table.Column<string>(type: "text", nullable: true),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_transfer_requests_AspNetUsers_initiating_employee_id",
                        column: x => x.initiating_employee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_requests_AspNetUsers_responding_employee_id",
                        column: x => x.responding_employee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_requests_stores_receiving_store_id",
                        column: x => x.receiving_store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_requests_stores_sending_store_id",
                        column: x => x.sending_store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transfer_request_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(26)", nullable: false),
                    transfer_request_id = table.Column<string>(type: "char(26)", nullable: false),
                    product_id = table.Column<string>(type: "char(26)", nullable: false),
                    product_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    quantity_requested = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_request_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_transfer_request_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_request_items_transfer_requests_transfer_request_id",
                        column: x => x.transfer_request_id,
                        principalTable: "transfer_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transfer_request_items_product_id",
                table: "transfer_request_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_request_items_transfer_request_id",
                table: "transfer_request_items",
                column: "transfer_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_initiating_employee_id",
                table: "transfer_requests",
                column: "initiating_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_receiving_store_id",
                table: "transfer_requests",
                column: "receiving_store_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_responding_employee_id",
                table: "transfer_requests",
                column: "responding_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_sending_store_id",
                table: "transfer_requests",
                column: "sending_store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transfer_request_items");

            migrationBuilder.DropTable(
                name: "transfer_requests");
        }
    }
}
