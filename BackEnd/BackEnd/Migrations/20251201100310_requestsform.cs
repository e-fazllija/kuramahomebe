using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class requestsform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomsNumber",
                table: "Requests");

            migrationBuilder.AddColumn<bool>(
                name: "Auction",
                table: "Requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EnergyClass",
                table: "Requests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                table: "Requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Furniture",
                table: "Requests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomsFrom",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomsTo",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Auction",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "EnergyClass",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Furniture",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RoomsFrom",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RoomsTo",
                table: "Requests");

            migrationBuilder.AddColumn<string>(
                name: "RoomsNumber",
                table: "Requests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
