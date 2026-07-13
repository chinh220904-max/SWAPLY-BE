using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIconUrlFromCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Categories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}