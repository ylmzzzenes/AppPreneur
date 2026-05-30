using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlow.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOnboardingProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Major",
                table: "Users",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademicGoal",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyStudyTargetMinutes",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnboardingCompleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicGoal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DailyStudyTargetMinutes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOnboardingCompleted",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Major",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }
    }
}
