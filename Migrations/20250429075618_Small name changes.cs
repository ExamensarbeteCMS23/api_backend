using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_backend.Migrations
{
    /// <inheritdoc />
    public partial class Smallnamechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Employees_CleanerId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CleanerId",
                table: "AspNetUsers",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CleanerId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Employees_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Employees_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "AspNetUsers",
                newName: "CleanerId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CleanerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Employees_CleanerId",
                table: "AspNetUsers",
                column: "CleanerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
