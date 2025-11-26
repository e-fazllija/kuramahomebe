using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class aggiuntoApplicationusersidACustomerseERequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_AgencyId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_AgencyId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "AgencyId",
                table: "Requests",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_AgencyId",
                table: "Requests",
                newName: "IX_Requests_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "AgencyId",
                table: "Customers",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Customer_AgencyId_CreationDate",
                table: "Customers",
                newName: "IX_Customer_ApplicationUserId_CreationDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Requests",
                newName: "AgencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_ApplicationUserId",
                table: "Requests",
                newName: "IX_Requests_AgencyId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Customers",
                newName: "AgencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Customer_ApplicationUserId_CreationDate",
                table: "Customers",
                newName: "IX_Customer_AgencyId_CreationDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_AgencyId",
                table: "Customers",
                column: "AgencyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_AgencyId",
                table: "Requests",
                column: "AgencyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
