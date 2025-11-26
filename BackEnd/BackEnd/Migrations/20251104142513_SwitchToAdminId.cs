using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToAdminId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AgencyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerNotes_AspNetUsers_ApplicationUserId",
                table: "CustomerNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_RealEstateProperties_AspNetUsers_AgentId",
                table: "RealEstateProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_RealEstatePropertyNotes_AspNetUsers_ApplicationUserId",
                table: "RealEstatePropertyNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestNotes_AspNetUsers_ApplicationUserId",
                table: "RequestNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Requests",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_ApplicationUserId",
                table: "Requests",
                newName: "IX_Requests_UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "RequestNotes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestNotes_ApplicationUserId",
                table: "RequestNotes",
                newName: "IX_RequestNotes_UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "RealEstatePropertyNotes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RealEstatePropertyNotes_ApplicationUserId",
                table: "RealEstatePropertyNotes",
                newName: "IX_RealEstatePropertyNotes_UserId");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "RealEstateProperties",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Provinces",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Locations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Customers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Customer_ApplicationUserId_CreationDate",
                table: "Customers",
                newName: "IX_Customer_UserId_CreationDate");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "CustomerNotes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerNotes_ApplicationUserId",
                table: "CustomerNotes",
                newName: "IX_CustomerNotes_UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Cities",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Calendars",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AgencyId",
                table: "AspNetUsers",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_AgencyId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AdminId",
                table: "AspNetUsers",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_UserId",
                table: "Calendars",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerNotes_AspNetUsers_UserId",
                table: "CustomerNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RealEstateProperties_AspNetUsers_UserId",
                table: "RealEstateProperties",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RealEstatePropertyNotes_AspNetUsers_UserId",
                table: "RealEstatePropertyNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestNotes_AspNetUsers_UserId",
                table: "RequestNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AdminId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_UserId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerNotes_AspNetUsers_UserId",
                table: "CustomerNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_RealEstateProperties_AspNetUsers_UserId",
                table: "RealEstateProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_RealEstatePropertyNotes_AspNetUsers_UserId",
                table: "RealEstatePropertyNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestNotes_AspNetUsers_UserId",
                table: "RequestNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Requests",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_UserId",
                table: "Requests",
                newName: "IX_Requests_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RequestNotes",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestNotes_UserId",
                table: "RequestNotes",
                newName: "IX_RequestNotes_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RealEstatePropertyNotes",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RealEstatePropertyNotes_UserId",
                table: "RealEstatePropertyNotes",
                newName: "IX_RealEstatePropertyNotes_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RealEstateProperties",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Provinces",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Locations",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Customers",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Customer_UserId_CreationDate",
                table: "Customers",
                newName: "IX_Customer_ApplicationUserId_CreationDate");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CustomerNotes",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerNotes_UserId",
                table: "CustomerNotes",
                newName: "IX_CustomerNotes_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Cities",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Calendars",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "AspNetUsers",
                newName: "AgencyId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_AdminId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AgencyId",
                table: "AspNetUsers",
                column: "AgencyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_ApplicationUserId",
                table: "Calendars",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerNotes_AspNetUsers_ApplicationUserId",
                table: "CustomerNotes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RealEstateProperties_AspNetUsers_AgentId",
                table: "RealEstateProperties",
                column: "AgentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RealEstatePropertyNotes_AspNetUsers_ApplicationUserId",
                table: "RealEstatePropertyNotes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestNotes_AspNetUsers_ApplicationUserId",
                table: "RequestNotes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
