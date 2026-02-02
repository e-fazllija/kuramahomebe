using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class StripePriceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verifica se la colonna esiste già prima di aggiungerla
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'SubscriptionPlans' 
                        AND column_name = 'StripePriceId'
                    ) THEN
                        ALTER TABLE ""SubscriptionPlans""
                        ADD COLUMN ""StripePriceId"" character varying(200);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePriceId",
                table: "SubscriptionPlans");
        }
    }
}
