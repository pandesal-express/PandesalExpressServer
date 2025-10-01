using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PandesalExpress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Drop_Payroll_And_Remove_Employee_Govt_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payrolls");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_pagibig_number",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_philhealth_number",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_sss_number",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_tin_number",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "pagibig_number",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "philhealth_number",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "sss_number",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "tin_number",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pagibig_number",
                table: "AspNetUsers",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "philhealth_number",
                table: "AspNetUsers",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sss_number",
                table: "AspNetUsers",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tin_number",
                table: "AspNetUsers",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payrolls",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(26)", nullable: false),
                    employee_id = table.Column<string>(type: "char(26)", nullable: false),
                    base_salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bonus = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    loan_deduction = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    overtime = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    pagibig_deduction = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    philhealth_deduction = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sss_deduction = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    tax = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrolls", x => x.id);
                    table.ForeignKey(
                        name: "FK_payrolls_AspNetUsers_employee_id",
                        column: x => x.employee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_pagibig_number",
                table: "AspNetUsers",
                column: "pagibig_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_philhealth_number",
                table: "AspNetUsers",
                column: "philhealth_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_sss_number",
                table: "AspNetUsers",
                column: "sss_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_tin_number",
                table: "AspNetUsers",
                column: "tin_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payrolls_employee_id",
                table: "payrolls",
                column: "employee_id");
        }
    }
}
