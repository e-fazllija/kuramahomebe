using BackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data
{
    public static class SubscriptionPlanSeeder
    {
        public static async Task SeedSubscriptionPlans(AppDbContext context)
        {
            // Verifica se i piani esistono già
            var existingPlans = await context.SubscriptionPlans
                .Where(p => p.Name.ToLower() == "free" || p.Name.ToLower() == "basic" || p.Name.ToLower() == "pro" || p.Name.ToLower() == "premium")
                .ToListAsync();

            if (existingPlans.Any())
            {
                Console.WriteLine("⚠️  I piani subscription esistono già. Eliminazione e ricreazione...");
                foreach (var plan in existingPlans)
                {
                    // Elimina le features associate
                    var features = await context.SubscriptionFeatures
                        .Where(f => f.SubscriptionPlanId == plan.Id)
                        .ToListAsync();
                    context.SubscriptionFeatures.RemoveRange(features);
                    context.SubscriptionPlans.Remove(plan);
                }
                await context.SaveChangesAsync();
            }

            // PIANO FREE (Trial di benvenuto)
            var freePlan = new SubscriptionPlan
            {
                Name = "Free",
                Description = "Piano gratuito di benvenuto per nuovi utenti. Periodo di prova di 10 giorni con funzionalità base.",
                Price = 0.00m,
                BillingPeriod = "monthly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(freePlan);
            await context.SaveChangesAsync();

            // Features Free (stesse del Basic per il trial)
            var freeFeatures = new List<SubscriptionFeature>
            {
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_agencies", FeatureValue = "1", Description = "Massimo 1 agenzia", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_agents", FeatureValue = "5", Description = "Massimo 5 agenti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_properties", FeatureValue = "20", Description = "Massimo 20 immobili", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_customers", FeatureValue = "50", Description = "Massimo 50 clienti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_requests", FeatureValue = "100", Description = "Massimo 100 richieste", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "export_enabled", FeatureValue = "false", Description = "Export dati disabilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_exports", FeatureValue = "0", Description = "Nessun export disponibile", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "storage_limit", FeatureValue = "1", Description = "Storage limitato a 1 GB", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };
            context.SubscriptionFeatures.AddRange(freeFeatures);

            // PIANO BASIC
            var basicPlan = new SubscriptionPlan
            {
                Name = "Basic",
                Description = "Piano base per piccole agenzie immobiliari. Ideale per iniziare con funzionalità essenziali.",
                Price = 12.00m,
                BillingPeriod = "monthly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(basicPlan);
            await context.SaveChangesAsync();

            // Features Basic
            var basicFeatures = new List<SubscriptionFeature>
            {
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_agencies", FeatureValue = "1", Description = "Massimo 1 agenzia", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_agents", FeatureValue = "5", Description = "Massimo 5 agenti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_properties", FeatureValue = "20", Description = "Massimo 20 immobili", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_customers", FeatureValue = "50", Description = "Massimo 50 clienti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_requests", FeatureValue = "100", Description = "Massimo 100 richieste", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "export_enabled", FeatureValue = "false", Description = "Export dati disabilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "max_exports", FeatureValue = "0", Description = "Nessun export disponibile", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = basicPlan.Id, FeatureName = "storage_limit", FeatureValue = "1", Description = "Storage limitato a 1 GB", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };
            context.SubscriptionFeatures.AddRange(basicFeatures);

            // PIANO PRO
            var proPlan = new SubscriptionPlan
            {
                Name = "Pro",
                Description = "Piano professionale per agenzie medie con più filiali. Include export dati e storage aumentato.",
                Price = 39.00m,
                BillingPeriod = "monthly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(proPlan);
            await context.SaveChangesAsync();

            // Features Pro
            var proFeatures = new List<SubscriptionFeature>
            {
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_agencies", FeatureValue = "5", Description = "Massimo 5 agenzie", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_agents", FeatureValue = "25", Description = "Massimo 25 agenti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_properties", FeatureValue = "100", Description = "Massimo 100 immobili", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_customers", FeatureValue = "500", Description = "Massimo 500 clienti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_requests", FeatureValue = "1000", Description = "Massimo 1000 richieste", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "export_enabled", FeatureValue = "true", Description = "Export dati abilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "max_exports", FeatureValue = "10", Description = "Massimo 10 export al mese", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = proPlan.Id, FeatureName = "storage_limit", FeatureValue = "10", Description = "Storage limitato a 10 GB", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };
            context.SubscriptionFeatures.AddRange(proFeatures);

            // PIANO PREMIUM
            var premiumPlan = new SubscriptionPlan
            {
                Name = "Premium",
                Description = "Piano enterprise per grandi agenzie e gruppi immobiliari. Funzionalità illimitate e supporto prioritario.",
                Price = 99.00m,
                BillingPeriod = "monthly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(premiumPlan);
            await context.SaveChangesAsync();

            // Features Premium
            var premiumFeatures = new List<SubscriptionFeature>
            {
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_agencies", FeatureValue = "unlimited", Description = "Agenzie illimitate", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_agents", FeatureValue = "unlimited", Description = "Agenti illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_properties", FeatureValue = "unlimited", Description = "Immobili illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_customers", FeatureValue = "unlimited", Description = "Clienti illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_requests", FeatureValue = "unlimited", Description = "Richieste illimitate", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "export_enabled", FeatureValue = "true", Description = "Export dati abilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_exports", FeatureValue = "unlimited", Description = "Export illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "storage_limit", FeatureValue = "unlimited", Description = "Storage illimitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };
            context.SubscriptionFeatures.AddRange(premiumFeatures);

            await context.SaveChangesAsync();

            Console.WriteLine("✅ Piani subscription creati con successo:");
            Console.WriteLine($"   - Free: €{freePlan.Price}/mese (Trial di benvenuto)");
            Console.WriteLine($"   - Basic: €{basicPlan.Price}/mese");
            Console.WriteLine($"   - Pro: €{proPlan.Price}/mese");
            Console.WriteLine($"   - Premium: €{premiumPlan.Price}/mese");
        }

        /// <summary>
        /// Verifica e crea il piano Free se non esiste (sempre necessario per i trial)
        /// </summary>
        public static async Task EnsureFreePlanExists(AppDbContext context)
        {
            var freePlan = await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Name.ToLower() == "free");

            if (freePlan == null)
            {
                Console.WriteLine("⚠️  Piano Free non trovato. Creazione...");
                
                freePlan = new SubscriptionPlan
                {
                    Name = "Free",
                    Description = "Piano gratuito di benvenuto per nuovi utenti. Periodo di prova di 10 giorni con funzionalità base.",
                    Price = 0.00m,
                    BillingPeriod = "monthly",
                    Active = true,
                    CreationDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                context.SubscriptionPlans.Add(freePlan);
                await context.SaveChangesAsync();

                // Features Free (stesse del Basic per il trial)
                var freeFeatures = new List<SubscriptionFeature>
                {
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_agencies", FeatureValue = "1", Description = "Massimo 1 agenzia", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_agents", FeatureValue = "5", Description = "Massimo 5 agenti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_properties", FeatureValue = "20", Description = "Massimo 20 immobili", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_customers", FeatureValue = "50", Description = "Massimo 50 clienti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_requests", FeatureValue = "100", Description = "Massimo 100 richieste", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "export_enabled", FeatureValue = "false", Description = "Export dati disabilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "max_exports", FeatureValue = "0", Description = "Nessun export disponibile", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                    new SubscriptionFeature { SubscriptionPlanId = freePlan.Id, FeatureName = "storage_limit", FeatureValue = "1", Description = "Storage limitato a 1 GB", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
                };
                context.SubscriptionFeatures.AddRange(freeFeatures);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Piano Free creato con successo (ID: {freePlan.Id})");
            }
            else
            {
                Console.WriteLine($"✅ Piano Free già esistente (ID: {freePlan.Id})");
            }
        }
    }
}

