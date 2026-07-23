using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBoostSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BoostExpiresAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoostPriority",
                table: "Listings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoostSubscriptionId",
                table: "Listings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BoostedAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BoostPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    MaxListings = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMonthlyQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalQuota = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    UsedQuota = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMonthlyQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMonthlyQuotas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoostPackageGoldenHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoostPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostPackageGoldenHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoostPackageGoldenHours_BoostPackages_BoostPackageId",
                        column: x => x.BoostPackageId,
                        principalTable: "BoostPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoostSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoostPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalQuota = table.Column<int>(type: "int", nullable: false),
                    UsedQuota = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoostSubscriptions_BoostPackages_BoostPackageId",
                        column: x => x.BoostPackageId,
                        principalTable: "BoostPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoostSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoostHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoostSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoostedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoostHistories_BoostSubscriptions_BoostSubscriptionId",
                        column: x => x.BoostSubscriptionId,
                        principalTable: "BoostSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoostHistories_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoostHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoostHistories_BoostSubscriptionId",
                table: "BoostHistories",
                column: "BoostSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostHistories_ExpiresAt",
                table: "BoostHistories",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BoostHistories_ListingId",
                table: "BoostHistories",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostHistories_UserId",
                table: "BoostHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostPackageGoldenHours_BoostPackageId",
                table: "BoostPackageGoldenHours",
                column: "BoostPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostSubscriptions_BoostPackageId",
                table: "BoostSubscriptions",
                column: "BoostPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostSubscriptions_UserId_Status",
                table: "BoostSubscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMonthlyQuotas_UserId_Year_Month",
                table: "UserMonthlyQuotas",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoostHistories");

            migrationBuilder.DropTable(
                name: "BoostPackageGoldenHours");

            migrationBuilder.DropTable(
                name: "UserMonthlyQuotas");

            migrationBuilder.DropTable(
                name: "BoostSubscriptions");

            migrationBuilder.DropTable(
                name: "BoostPackages");

            migrationBuilder.DropColumn(
                name: "BoostExpiresAt",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BoostPriority",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BoostSubscriptionId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BoostedAt",
                table: "Listings");
        }
    }
}
