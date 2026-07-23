using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamePremiumPlansToPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PremiumPlans_PremiumPlanId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "PremiumPlans");

            migrationBuilder.RenameColumn(
                name: "PremiumPlanId",
                table: "Subscriptions",
                newName: "PackageId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_PremiumPlanId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_PackageId");

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    MaxListings = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Description", "DurationDays", "IsActive", "MaxListings", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Basic plan with 5 listings limit", 30, true, 5, "Basic", 99000m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Premium plan with unlimited listings", 30, true, 999, "Premium", 199000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Name",
                table: "Packages",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Packages_PackageId",
                table: "Subscriptions",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Packages_PackageId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.RenameColumn(
                name: "PackageId",
                table: "Subscriptions",
                newName: "PremiumPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_PackageId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_PremiumPlanId");

            migrationBuilder.CreateTable(
                name: "PremiumPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxListings = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PremiumPlans",
                columns: new[] { "Id", "Description", "DurationDays", "IsActive", "MaxListings", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Basic plan with 5 listings limit", 30, true, 5, "Basic", 99000m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Premium plan with unlimited listings", 30, true, 999, "Premium", 199000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPlans_Name",
                table: "PremiumPlans",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PremiumPlans_PremiumPlanId",
                table: "Subscriptions",
                column: "PremiumPlanId",
                principalTable: "PremiumPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
