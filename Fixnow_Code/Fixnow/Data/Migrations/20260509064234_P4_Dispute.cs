using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class P4_Dispute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_disputes_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_disputes_users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_disputes_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dispute_evidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispute_evidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dispute_evidences_disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispute_evidences_users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    RefundType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refunds_disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_refunds_users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dispute_evidences_DisputeId",
                table: "dispute_evidences",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_dispute_evidences_UploadedBy",
                table: "dispute_evidences",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_disputes_BookingId",
                table: "disputes",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_disputes_CustomerId",
                table: "disputes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_disputes_WorkerId",
                table: "disputes",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_DisputeId",
                table: "refunds",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_ProcessedBy",
                table: "refunds",
                column: "ProcessedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispute_evidences");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "disputes");
        }
    }
}
