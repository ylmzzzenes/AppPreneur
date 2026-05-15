using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlow.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusScanSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyllabusScanSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CourseTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceTextHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PreviewJson = table.Column<string>(type: "nvarchar(max)", maxLength: 512000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusScanSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusScanSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusScanSessions_UserId_ExpiresAt",
                table: "SyllabusScanSessions",
                columns: new[] { "UserId", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyllabusScanSessions");
        }
    }
}
