using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeCompletionConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Exchanges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposerConfirmedAt",
                table: "Exchanges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProposerConfirmedComplete",
                table: "Exchanges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceiverConfirmedAt",
                table: "Exchanges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiverConfirmedComplete",
                table: "Exchanges",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "ProposerConfirmedAt",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "ProposerConfirmedComplete",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "ReceiverConfirmedAt",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "ReceiverConfirmedComplete",
                table: "Exchanges");
        }
    }
}
