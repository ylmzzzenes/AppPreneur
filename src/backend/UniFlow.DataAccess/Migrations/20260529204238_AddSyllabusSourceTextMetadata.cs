using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlow.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusSourceTextMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreviewJson",
                table: "SyllabusScanSessions",
                type: "nvarchar(max)",
                maxLength: 20000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 20000);

            migrationBuilder.AddColumn<int>(
                name: "SourceTextLength",
                table: "SyllabusScanSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceText",
                table: "Syllabi",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceTextLength",
                table: "Syllabi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusScanSessions_SourceTextHash",
                table: "SyllabusScanSessions",
                column: "SourceTextHash");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabi_SourceTextHash",
                table: "Syllabi",
                column: "SourceTextHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyllabusScanSessions_SourceTextHash",
                table: "SyllabusScanSessions");

            migrationBuilder.DropIndex(
                name: "IX_Syllabi_SourceTextHash",
                table: "Syllabi");

            migrationBuilder.DropColumn(
                name: "SourceTextLength",
                table: "SyllabusScanSessions");

            migrationBuilder.DropColumn(
                name: "SourceText",
                table: "Syllabi");

            migrationBuilder.DropColumn(
                name: "SourceTextLength",
                table: "Syllabi");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewJson",
                table: "SyllabusScanSessions",
                type: "nvarchar(max)",
                maxLength: 20000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 20000,
                oldNullable: true);
        }
    }
}
