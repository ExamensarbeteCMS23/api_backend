using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_backend.Migrations
{
    /// <inheritdoc />
    public partial class Smallnamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cleaners_CleanerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingCleaner_Cleaners_CleanerId",
                table: "BookingCleaner");

            migrationBuilder.DropForeignKey(
                name: "FK_Cleaners_Roles_RoleId",
                table: "Cleaners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cleaners",
                table: "Cleaners");

            migrationBuilder.RenameTable(
                name: "Cleaners",
                newName: "Employees");

            migrationBuilder.RenameIndex(
                name: "IX_Cleaners_RoleId",
                table: "Employees",
                newName: "IX_Employees_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Employees_CleanerId",
                table: "AspNetUsers",
                column: "CleanerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCleaner_Employees_CleanerId",
                table: "BookingCleaner",
                column: "CleanerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Employees_CleanerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingCleaner_Employees_CleanerId",
                table: "BookingCleaner");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Cleaners");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_RoleId",
                table: "Cleaners",
                newName: "IX_Cleaners_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cleaners",
                table: "Cleaners",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cleaners_CleanerId",
                table: "AspNetUsers",
                column: "CleanerId",
                principalTable: "Cleaners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCleaner_Cleaners_CleanerId",
                table: "BookingCleaner",
                column: "CleanerId",
                principalTable: "Cleaners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cleaners_Roles_RoleId",
                table: "Cleaners",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
