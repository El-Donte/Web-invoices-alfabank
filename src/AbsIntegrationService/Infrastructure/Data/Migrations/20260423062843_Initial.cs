using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbsIntegrationService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "drafts");

            migrationBuilder.CreateTable(
                name: "invoice_drafts",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    seller_inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    seller_kpp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    seller_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    seller_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    buyer_inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    buyer_kpp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    buyer_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    buyer_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    total_without_nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_with_nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_drafts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "draft_operation_links",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    source_message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_operation_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_operation_links_invoice_drafts_invoice_draft_id",
                        column: x => x.invoice_draft_id,
                        principalSchema: "drafts",
                        principalTable: "invoice_drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_draft_lines",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price_without_nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    amount_without_nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nds_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_with_Nds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    contract_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_draft_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_draft_lines_invoice_drafts_invoice_draft_id",
                        column: x => x.invoice_draft_id,
                        principalSchema: "drafts",
                        principalTable: "invoice_drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_draft_operation_links_invoice_draft_id",
                schema: "drafts",
                table: "draft_operation_links",
                column: "invoice_draft_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_draft_lines_invoice_draft_id",
                schema: "drafts",
                table: "invoice_draft_lines",
                column: "invoice_draft_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_drafts_operation_number",
                schema: "drafts",
                table: "invoice_drafts",
                column: "operation_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_operation_links",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "invoice_draft_lines",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "invoice_drafts",
                schema: "drafts");
        }
    }
}
