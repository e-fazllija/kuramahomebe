using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemovedShortTitleAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "RealEstateProperties");

            migrationBuilder.DropColumn(
                name: "ShortTitle",
                table: "RealEstateProperties");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Requests");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "RealEstateProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortTitle",
                table: "RealEstateProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
