using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddedCalendarId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalendarId",
                table: "RequestNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalendarId",
                table: "RealEstatePropertyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalendarId",
                table: "CustomerNotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "RequestNotes");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "RealEstatePropertyNotes");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "CustomerNotes");
        }
    }
}
