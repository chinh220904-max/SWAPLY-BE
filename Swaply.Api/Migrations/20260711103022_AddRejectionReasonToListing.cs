using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Listings");
        }
    }
}
