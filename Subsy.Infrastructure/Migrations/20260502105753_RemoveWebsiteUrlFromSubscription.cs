using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subsy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWebsiteUrlFromSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Subscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Subscriptions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
