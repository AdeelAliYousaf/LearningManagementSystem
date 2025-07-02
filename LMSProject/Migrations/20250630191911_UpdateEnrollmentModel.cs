using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrollmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_AspNetUsers_StudentId",
                table: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Enrollments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                newName: "IX_Enrollments_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_AspNetUsers_UserId",
                table: "Enrollments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_AspNetUsers_UserId",
                table: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Enrollments",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_UserId",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_AspNetUsers_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
