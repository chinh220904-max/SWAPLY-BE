using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swaply.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonthlyQuotaTo10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing quota records to 10 (only current month, don't touch used quotas)
            migrationBuilder.Sql(@"
                UPDATE [UserMonthlyQuotas]
                SET [TotalQuota] = 10
                WHERE [TotalQuota] < 10
                  AND [Year] = YEAR(GETUTCDATE())
                  AND [Month] = MONTH(GETUTCDATE());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [UserMonthlyQuotas]
                SET [TotalQuota] = 3
                WHERE [TotalQuota] = 10
                  AND [Year] = YEAR(GETUTCDATE())
                  AND [Month] = MONTH(GETUTCDATE());
            ");
        }
    }
}
