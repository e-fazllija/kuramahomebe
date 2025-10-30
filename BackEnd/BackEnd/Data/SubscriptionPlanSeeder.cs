using BackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data
{
    public static class SubscriptionPlanSeeder
    {
        public static async Task SeedSubscriptionPlans(AppDbContext context)
        {
            // Controlla se esistono già piani
            if (await context.SubscriptionPlans.AnyAsync())
            {
                Console.WriteLine("Piani di sottoscrizione già presenti nel database. Skip seed.");
                return;
            }

            var plans = new List<SubscriptionPlan>
            {
                // Piano Basic
                new SubscriptionPlan
                {
                    Name = "basic",
                    Description = "Perfetto per iniziare - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore.",
                    Price = 19.00M,
                    BillingPeriod = "monthly",
                    Active = true
                },
                
                // Piano Pro
                new SubscriptionPlan
                {
                    Name = "pro",
                    Description = "La scelta migliore per professionisti - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore.",
                    Price = 49.00M,
                    BillingPeriod = "monthly",
                    Active = true
                },
                
                // Piano Premium
                new SubscriptionPlan
                {
                    Name = "premium",
                    Description = "Soluzioni aziendali complete - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore.",
                    Price = 99.00M,
                    BillingPeriod = "monthly",
                    Active = true
                }
            };

            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seed completato: {plans.Count} piani di sottoscrizione creati.");
            Console.WriteLine("- Basic: €19/mese");
            Console.WriteLine("- Pro: €49/mese");
            Console.WriteLine("- Premium: €99/mese");
        }
    }
}

