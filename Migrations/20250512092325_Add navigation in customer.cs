using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_backend.Migrations
{
    /// <inheritdoc />
    public partial class Addnavigationincustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerAddresses_AddressId",
                table: "Customers");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerAddresses_AddressId",
                table: "Customers",
                column: "AddressId",
                principalTable: "CustomerAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerAddresses_AddressId",
                table: "Customers");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerAddresses_AddressId",
                table: "Customers",
                column: "AddressId",
                principalTable: "CustomerAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
