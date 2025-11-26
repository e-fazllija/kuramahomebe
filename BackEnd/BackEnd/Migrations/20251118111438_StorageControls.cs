using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class StorageControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nota: CustomerId non esiste più nell'entità RealEstatePropertyNotes
            // Rimossa la rimozione della colonna perché non è presente nel modello corrente
            
            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "Documentation",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StorageUsedBytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ExportHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExportHistory_UserId",
                table: "ExportHistory",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportHistory");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "Documentation");

            migrationBuilder.DropColumn(
                name: "StorageUsedBytes",
                table: "AspNetUsers");

            // Nota: CustomerId non viene riaggiunto perché non esiste nell'entità corrente
            // migrationBuilder.AddColumn<int>(
            //     name: "CustomerId",
            //     table: "RealEstatePropertyNotes",
            //     type: "integer",
            //     nullable: true);
        }
    }
}
