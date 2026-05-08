using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "worker_kyc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenIdNumber = table.Column<string>(type: "text", nullable: false),
                    CitizenFrontUrl = table.Column<string>(type: "text", nullable: false),
                    CitizenBackUrl = table.Column<string>(type: "text", nullable: false),
                    SelfieUrl = table.Column<string>(type: "text", nullable: false),
                    CertificateUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_kyc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_worker_kyc_users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_worker_kyc_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_location_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<Point>(type: "geography(Point, 4326)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_location_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_worker_location_histories_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: false),
                    TotalJobs = table.Column<int>(type: "integer", nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_worker_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_worker_reviews_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_worker_reviews_users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_worker_reviews_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_services",
                columns: table => new
                {
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_services", x => new { x.WorkerId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_worker_services_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_worker_services_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_worker_kyc_VerifiedBy",
                table: "worker_kyc",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_worker_kyc_WorkerId",
                table: "worker_kyc",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_location_histories_WorkerId",
                table: "worker_location_histories",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_reviews_BookingId",
                table: "worker_reviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_reviews_CustomerId",
                table: "worker_reviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_reviews_WorkerId",
                table: "worker_reviews",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_services_ServiceId",
                table: "worker_services",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worker_kyc");

            migrationBuilder.DropTable(
                name: "worker_location_histories");

            migrationBuilder.DropTable(
                name: "worker_profiles");

            migrationBuilder.DropTable(
                name: "worker_reviews");

            migrationBuilder.DropTable(
                name: "worker_services");
        }
    }
}
