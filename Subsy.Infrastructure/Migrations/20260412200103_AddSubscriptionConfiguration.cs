using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subsy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_IsArchived_RenewalDate",
                table: "Subscriptions",
                columns: new[] { "UserId", "IsArchived", "RenewalDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId_IsArchived_RenewalDate",
                table: "Subscriptions");
        }
    }
}
