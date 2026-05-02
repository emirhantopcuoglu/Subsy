using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subsy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndWebsiteUrlToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Subscriptions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Subscriptions");
        }
    }
}
