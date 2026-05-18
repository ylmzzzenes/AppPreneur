using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlow.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusSourceTextHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceTextHash",
                table: "Syllabi",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTextPreview",
                table: "Syllabi",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Syllabi
                SET SourceTextPreview = LEFT(SourceText, 4000)
                WHERE SourceText IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "SourceText",
                table: "Syllabi");

            migrationBuilder.Sql(
                """
                UPDATE SyllabusScanSessions
                SET PreviewJson = LEFT(PreviewJson, 20000)
                WHERE LEN(PreviewJson) > 20000
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceText",
                table: "Syllabi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Syllabi
                SET SourceText = SourceTextPreview
                WHERE SourceTextPreview IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "SourceTextHash",
                table: "Syllabi");

            migrationBuilder.DropColumn(
                name: "SourceTextPreview",
                table: "Syllabi");
        }
    }
}
