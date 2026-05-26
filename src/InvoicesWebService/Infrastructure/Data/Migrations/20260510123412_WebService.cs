using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoicesWebService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class WebService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "draft_invoice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    total_nds_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    total_with_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    TotalWithoutNds = table.Column<decimal>(type: "numeric", nullable: false),
                    currency_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    validation_error = table.Column<string>(type: "text", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_invoice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "draft_invoice_line",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    product_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    amount_without_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    nds_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    draft_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_transaction_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_invoice_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_invoice_line_draft_invoice_draft_invoice_id",
                        column: x => x.draft_invoice_id,
                        principalTable: "draft_invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    total_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    total_with_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    TotalWithoutNds = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    payment_doc_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_doc_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sequence_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    current_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_draft_invoice_draft_invoice_id",
                        column: x => x.draft_invoice_id,
                        principalTable: "draft_invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invoice_user_last_author_id",
                        column: x => x.last_author_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invoice_field_change_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: false),
                    new_value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_field_change_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_field_change_history_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_field_change_history_user_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invoice_line",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    product_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    amount_without_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    amount_with_nds = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    nds_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_line_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_draft_invoice_group_id",
                table: "draft_invoice",
                column: "group_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_invoice_line_draft",
                table: "draft_invoice_line",
                column: "draft_invoice_id");

            migrationBuilder.CreateIndex(
                name: "UX_draft_invoice_line_raw_tx",
                table: "draft_invoice_line",
                column: "raw_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_last_author_id",
                table: "invoice",
                column: "last_author_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_status_date",
                table: "invoice",
                columns: new[] { "status", "issue_date" });

            migrationBuilder.CreateIndex(
                name: "UX_invoice_draft_id",
                table: "invoice",
                column: "draft_invoice_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_invoice_number",
                table: "invoice",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_change_history_invoice",
                table: "invoice_field_change_history",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_field_change_history_changed_by_id",
                table: "invoice_field_change_history",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_line_invoice",
                table: "invoice_line",
                column: "invoice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_invoice_line");

            migrationBuilder.DropTable(
                name: "invoice_field_change_history");

            migrationBuilder.DropTable(
                name: "invoice_line");

            migrationBuilder.DropTable(
                name: "invoice");

            migrationBuilder.DropTable(
                name: "draft_invoice");
        }
    }
}
