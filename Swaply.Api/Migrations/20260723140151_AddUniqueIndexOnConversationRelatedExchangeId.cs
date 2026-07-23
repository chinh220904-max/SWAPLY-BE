using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnConversationRelatedExchangeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_RelatedExchangeId",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_RelatedExchangeId_Unique",
                table: "Conversations",
                column: "RelatedExchangeId",
                unique: true,
                filter: "RelatedExchangeId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_RelatedExchangeId_Unique",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_RelatedExchangeId",
                table: "Conversations",
                column: "RelatedExchangeId");
        }
    }
}
