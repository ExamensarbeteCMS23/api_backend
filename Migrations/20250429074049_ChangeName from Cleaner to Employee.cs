using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNamefromCleanertoEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CleanerPhone",
                table: "Cleaners",
                newName: "EmployeePhone");

            migrationBuilder.RenameColumn(
                name: "CleanerLastName",
                table: "Cleaners",
                newName: "EmployeeLastName");

            migrationBuilder.RenameColumn(
                name: "CleanerFirstName",
                table: "Cleaners",
                newName: "EmployeeFirstName");

            migrationBuilder.RenameColumn(
                name: "CleanerEmail",
                table: "Cleaners",
                newName: "EmployeeEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeePhone",
                table: "Cleaners",
                newName: "CleanerPhone");

            migrationBuilder.RenameColumn(
                name: "EmployeeLastName",
                table: "Cleaners",
                newName: "CleanerLastName");

            migrationBuilder.RenameColumn(
                name: "EmployeeFirstName",
                table: "Cleaners",
                newName: "CleanerFirstName");

            migrationBuilder.RenameColumn(
                name: "EmployeeEmail",
                table: "Cleaners",
                newName: "CleanerEmail");
        }
    }
}
