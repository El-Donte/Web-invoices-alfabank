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
                name: "public");

            migrationBuilder.CreateTable(
                name: "counterparty",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    inn = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    kpp = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterparty", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "department",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aggregation_group",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    shipment_count = table.Column<int>(type: "integer", nullable: false),
                    advance_count = table.Column<int>(type: "integer", nullable: false),
                    corrective_count = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    validation_error = table.Column<string>(type: "text", nullable: false),
                    last_processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ready_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aggregation_group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aggregation_group_counterparty_counterparty_id",
                        column: x => x.counterparty_id,
                        principalSchema: "public",
                        principalTable: "counterparty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_aggregation_group_department_department_id",
                        column: x => x.department_id,
                        principalSchema: "public",
                        principalTable: "department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "raw_transaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    unit_measure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    nds_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    nds_amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(16,4)", precision: 16, scale: 4, nullable: false),
                    payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    validation_error = table.Column<string>(type: "text", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aggregation_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_raw_transaction_aggregation_group_aggregation_group_id",
                        column: x => x.aggregation_group_id,
                        principalSchema: "public",
                        principalTable: "aggregation_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_raw_transaction_counterparty_counterparty_id",
                        column: x => x.counterparty_id,
                        principalSchema: "public",
                        principalTable: "counterparty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_raw_transaction_department_department_id",
                        column: x => x.department_id,
                        principalSchema: "public",
                        principalTable: "department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "processing_error",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    stage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    retryable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    aggregation_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    draft_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_error", x => x.Id);
                    table.ForeignKey(
                        name: "FK_processing_error_aggregation_group_aggregation_group_id",
                        column: x => x.aggregation_group_id,
                        principalSchema: "public",
                        principalTable: "aggregation_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_processing_error_raw_transaction_raw_transaction_id",
                        column: x => x.raw_transaction_id,
                        principalSchema: "public",
                        principalTable: "raw_transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aggregation_group_counterparty_id",
                schema: "public",
                table: "aggregation_group",
                column: "counterparty_id");

            migrationBuilder.CreateIndex(
                name: "IX_aggregation_group_department_id",
                schema: "public",
                table: "aggregation_group",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_aggregation_group_status_date",
                schema: "public",
                table: "aggregation_group",
                columns: new[] { "status", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "UX_aggregation_group_drf",
                schema: "public",
                table: "aggregation_group",
                column: "operation_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_counterparty_inn",
                schema: "public",
                table: "counterparty",
                column: "inn");

            migrationBuilder.CreateIndex(
                name: "UX_department_code",
                schema: "public",
                table: "department",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processing_error_aggregation_group_id",
                schema: "public",
                table: "processing_error",
                column: "aggregation_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_error_raw_transaction_id",
                schema: "public",
                table: "processing_error",
                column: "raw_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_error_retries",
                schema: "public",
                table: "processing_error",
                columns: new[] { "resolved", "retryable", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_raw_transaction_aggregation_group_id",
                schema: "public",
                table: "raw_transaction",
                column: "aggregation_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_raw_transaction_counterparty_id",
                schema: "public",
                table: "raw_transaction",
                column: "counterparty_id");

            migrationBuilder.CreateIndex(
                name: "IX_raw_transaction_department_id",
                schema: "public",
                table: "raw_transaction",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_rawtransaction_drf_type",
                schema: "public",
                table: "raw_transaction",
                columns: new[] { "operation_number", "type" });

            migrationBuilder.CreateIndex(
                name: "IX_rawtransaction_status_received",
                schema: "public",
                table: "raw_transaction",
                columns: new[] { "status", "received_at" });

            migrationBuilder.CreateIndex(
                name: "UX_rawtransaction_payload_hash",
                schema: "public",
                table: "raw_transaction",
                column: "payload_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processing_error",
                schema: "public");

            migrationBuilder.DropTable(
                name: "raw_transaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "aggregation_group",
                schema: "public");

            migrationBuilder.DropTable(
                name: "counterparty",
                schema: "public");

            migrationBuilder.DropTable(
                name: "department",
                schema: "public");
        }
    }
}
