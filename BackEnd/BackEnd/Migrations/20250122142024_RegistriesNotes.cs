using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RegistriesNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars");

            migrationBuilder.AddColumn<bool>(
                name: "MortgageAdviceRequired",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "RealEstatePropertyPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AcquisitionDone",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OngoingAssignment",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Calendars",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RealEstatePropertyId",
                table: "Calendars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "Calendars",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_RealEstatePropertyId",
                table: "Calendars",
                column: "RealEstatePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_RequestId",
                table: "Calendars",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_RealEstateProperties_RealEstatePropertyId",
                table: "Calendars",
                column: "RealEstatePropertyId",
                principalTable: "RealEstateProperties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Requests_RequestId",
                table: "Calendars",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_RealEstateProperties_RealEstatePropertyId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Requests_RequestId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_RealEstatePropertyId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_RequestId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "MortgageAdviceRequired",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "RealEstatePropertyPhotos");

            migrationBuilder.DropColumn(
                name: "AcquisitionDone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OngoingAssignment",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RealEstatePropertyId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Calendars");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Calendars",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Customers_CustomerId",
                table: "Calendars",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
