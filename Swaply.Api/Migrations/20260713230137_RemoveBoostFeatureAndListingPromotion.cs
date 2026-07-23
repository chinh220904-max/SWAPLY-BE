using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBoostFeatureAndListingPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBoosts");

            migrationBuilder.DropTable(
                name: "BoostPackages");

            migrationBuilder.DropColumn(
                name: "IsPromoted",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PromotedUntil",
                table: "Listings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPromoted",
                table: "Listings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PromotedUntil",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BoostPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PeakSlots = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, defaultValue: "[]"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    QuotaListings = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBoosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoostPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    QuotaListingsUsed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBoosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBoosts_BoostPackages_BoostPackageId",
                        column: x => x.BoostPackageId,
                        principalTable: "BoostPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBoosts_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBoosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBoosts_BoostPackageId",
                table: "UserBoosts",
                column: "BoostPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBoosts_ExpiresAt",
                table: "UserBoosts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserBoosts_ListingId",
                table: "UserBoosts",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBoosts_UserId",
                table: "UserBoosts",
                column: "UserId");
        }
    }
}
