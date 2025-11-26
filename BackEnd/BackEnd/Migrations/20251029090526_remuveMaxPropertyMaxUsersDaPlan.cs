using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class remuveMaxPropertyMaxUsersDaPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxProperties",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "SubscriptionPlans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxProperties",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: true);
        }
    }
}
