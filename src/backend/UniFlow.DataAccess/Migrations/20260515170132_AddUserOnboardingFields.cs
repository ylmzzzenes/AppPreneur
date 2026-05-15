using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlow.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Major",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalityVibe",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Friendly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Major",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonalityVibe",
                table: "Users");
        }
    }
}
