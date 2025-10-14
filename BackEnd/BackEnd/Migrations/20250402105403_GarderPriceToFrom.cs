using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class GarderPriceToFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Requests",
                newName: "PriceTo");

            migrationBuilder.AddColumn<string>(
                name: "AgencyId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GardenFrom",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GardenTo",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PriceFrom",
                table: "Requests",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MQGarden",
                table: "RealEstateProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PriceReduced",
                table: "RealEstateProperties",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "AgencyId",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Cancelled",
                table: "Calendars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                table: "Calendars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Postponed",
                table: "Calendars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AgencyId",
                table: "Requests",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AgencyId",
                table: "Customers",
                column: "AgencyId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_AgencyId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_AgencyId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AgencyId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AgencyId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "GardenFrom",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "GardenTo",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PriceFrom",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "MQGarden",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "PriceReduced",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Cancelled",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Confirmed",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Postponed",
                table: "Calendars");

            migrationBuilder.RenameColumn(
                name: "PriceTo",
                table: "Requests",
                newName: "Price");
        }
    }
}
