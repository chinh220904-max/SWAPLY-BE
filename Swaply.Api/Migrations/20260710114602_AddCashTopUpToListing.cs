using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCashTopUpToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CashTopUpAmount",
                table: "Listings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CashTopUpCurrency",
                table: "Listings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "VND");

            migrationBuilder.AddColumn<string>(
                name: "ExchangeWish",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FavoriteCount",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "CashTopUpAmount",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "CashTopUpCurrency",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ExchangeWish",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "FavoriteCount",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Listings");
        }
    }
}
