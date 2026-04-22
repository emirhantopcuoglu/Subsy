using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subsy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Subscriptions",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "TRY");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Subscriptions");
        }
    }
}
