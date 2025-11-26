using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentationFoldersAndPrivacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgencyId",
                table: "Documentation",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Documentation",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFolder",
                table: "Documentation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Documentation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentPath",
                table: "Documentation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Documentation",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "IsFolder",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "ParentPath",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Documentation");
        }
    }
}
