using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class Logup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalCode",
                table: "AspNetUsers",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PEC",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SDICode",
                table: "AspNetUsers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VATNumber",
                table: "AspNetUsers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FiscalCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PEC",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SDICode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VATNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "AspNetUsers");
        }
    }
}
