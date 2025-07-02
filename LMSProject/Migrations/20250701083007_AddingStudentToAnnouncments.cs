using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSProject.Migrations
{
    /// <inheritdoc />
    public partial class AddingStudentToAnnouncments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Announcements");
        }
    }
}
