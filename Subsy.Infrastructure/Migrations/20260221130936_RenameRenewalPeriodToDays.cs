using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subsy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameRenewalPeriodToDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenewalPeriod",
                table: "Subscriptions");

            migrationBuilder.AddColumn<int>(
                name: "RenewalPeriodDays",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenewalPeriodDays",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "RenewalPeriod",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
