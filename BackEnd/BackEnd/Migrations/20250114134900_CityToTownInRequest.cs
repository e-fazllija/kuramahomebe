using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class CityToTownInRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Requests",
                newName: "Town");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Calendars",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Calendars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Calendars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_CustomerId",
                table: "Calendars",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_CustomerId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Calendars");

            migrationBuilder.RenameColumn(
                name: "Town",
                table: "Requests",
                newName: "City");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Calendars",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
