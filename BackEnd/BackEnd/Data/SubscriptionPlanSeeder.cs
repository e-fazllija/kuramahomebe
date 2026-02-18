using BackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data
{
    public static class SubscriptionPlanSeeder
    {
        public static async Task SeedSubscriptionPlans(AppDbContext context)
        {
            // Verifica se i piani esistono già (base e prepagati)
            var planNames = new[] { 
                "free", "basic", "pro", "premium",
                "basic 3 months", "basic 6 months", "basic 12 months",
                "pro 3 months", "pro 6 months", "pro 12 months",
                "premium 3 months", "premium 6 months", "premium 12 months"
            };
            
            var existingPlans = await context.SubscriptionPlans
                .Where(p => planNames.Contains(p.Name.ToLower()))
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
                Price = 19.00m,
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

            // Features Premium (max 10 agenzie, 50 agenti)
            var premiumFeatures = new List<SubscriptionFeature>
            {
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_agencies", FeatureValue = "10", Description = "Massimo 10 agenzie", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_agents", FeatureValue = "50", Description = "Massimo 50 agenti", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_properties", FeatureValue = "unlimited", Description = "Immobili illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_customers", FeatureValue = "unlimited", Description = "Clienti illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_requests", FeatureValue = "unlimited", Description = "Richieste illimitate", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "export_enabled", FeatureValue = "true", Description = "Export dati abilitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "max_exports", FeatureValue = "unlimited", Description = "Export illimitati", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new SubscriptionFeature { SubscriptionPlanId = premiumPlan.Id, FeatureName = "storage_limit", FeatureValue = "unlimited", Description = "Storage illimitato", CreationDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };
            context.SubscriptionFeatures.AddRange(premiumFeatures);

            // ========== PIANI PREPAGATI MULTI-MESE ==========
            // I prepagati NON hanno features proprie: UserSubscriptionServices le ricava sempre da Basic/Pro/Premium mensile.
            
            // BASIC 3 MONTHS
            var basic3Months = new SubscriptionPlan
            {
                Name = "Basic 3 Months",
                Description = "Piano Basic prepagato valido 3 mesi. Ideale per piccole realtà che vogliono iniziare con un pagamento unico.",
                Price = 54.00m,
                BillingPeriod = "quarterly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(basic3Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Basic

            // BASIC 6 MONTHS
            var basic6Months = new SubscriptionPlan
            {
                Name = "Basic 6 Months",
                Description = "Piano Basic prepagato valido 6 mesi. Pensato per un utilizzo continuativo con maggiore risparmio rispetto al mensile.",
                Price = 102.00m,
                BillingPeriod = "semiannual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(basic6Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Basic

            // BASIC 12 MONTHS
            var basic12Months = new SubscriptionPlan
            {
                Name = "Basic 12 Months",
                Description = "Piano Basic prepagato valido 12 mesi. La soluzione più conveniente per piccole realtà che vogliono stabilità.",
                Price = 182.00m,
                BillingPeriod = "annual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(basic12Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Basic

            // PRO 3 MONTHS
            var pro3Months = new SubscriptionPlan
            {
                Name = "Pro 3 Months",
                Description = "Piano Pro prepagato valido 3 mesi. Ideale per agenzie strutturate che vogliono testare il piano con pagamento unico.",
                Price = 111.00m,
                BillingPeriod = "quarterly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(pro3Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Pro

            // PRO 6 MONTHS
            var pro6Months = new SubscriptionPlan
            {
                Name = "Pro 6 Months",
                Description = "Piano Pro prepagato valido 6 mesi. Scelta professionale per agenzie che utilizzano il gestionale ogni giorno.",
                Price = 210.00m,
                BillingPeriod = "semiannual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(pro6Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Pro

            // PRO 12 MONTHS
            var pro12Months = new SubscriptionPlan
            {
                Name = "Pro 12 Months",
                Description = "Piano Pro prepagato valido 12 mesi. Miglior rapporto prezzo/tempo per agenzie che vogliono continuità.",
                Price = 374.00m,
                BillingPeriod = "annual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(pro12Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Pro

            // PREMIUM 3 MONTHS
            var premium3Months = new SubscriptionPlan
            {
                Name = "Premium 3 Months",
                Description = "Piano Premium prepagato valido 3 mesi. Accesso completo senza limiti, ideale per realtà in crescita.",
                Price = 282.00m,
                BillingPeriod = "quarterly",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(premium3Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Premium

            // PREMIUM 6 MONTHS
            var premium6Months = new SubscriptionPlan
            {
                Name = "Premium 6 Months",
                Description = "Piano Premium prepagato valido 6 mesi. Tutte le funzionalità senza limiti con un risparmio significativo.",
                Price = 535.00m,
                BillingPeriod = "semiannual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(premium6Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Premium

            // PREMIUM 12 MONTHS
            var premium12Months = new SubscriptionPlan
            {
                Name = "Premium 12 Months",
                Description = "Piano Premium prepagato valido 12 mesi. La soluzione completa e più conveniente per chi vuole il massimo.",
                Price = 950.00m,
                BillingPeriod = "annual",
                Active = true,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.SubscriptionPlans.Add(premium12Months);
            await context.SaveChangesAsync();
            // Prepagate: nessuna feature propria, usa quelle del Premium

            await context.SaveChangesAsync();

            Console.WriteLine("✅ Piani subscription creati con successo:");
            Console.WriteLine($"   - Free: €{freePlan.Price}/mese (Trial di benvenuto)");
            Console.WriteLine($"   - Basic: €{basicPlan.Price}/mese");
            Console.WriteLine($"   - Basic 3 Months: €{basic3Months.Price} (prepagato 3 mesi)");
            Console.WriteLine($"   - Basic 6 Months: €{basic6Months.Price} (prepagato 6 mesi)");
            Console.WriteLine($"   - Basic 12 Months: €{basic12Months.Price} (prepagato 12 mesi)");
            Console.WriteLine($"   - Pro: €{proPlan.Price}/mese");
            Console.WriteLine($"   - Pro 3 Months: €{pro3Months.Price} (prepagato 3 mesi)");
            Console.WriteLine($"   - Pro 6 Months: €{pro6Months.Price} (prepagato 6 mesi)");
            Console.WriteLine($"   - Pro 12 Months: €{pro12Months.Price} (prepagato 12 mesi)");
            Console.WriteLine($"   - Premium: €{premiumPlan.Price}/mese");
            Console.WriteLine($"   - Premium 3 Months: €{premium3Months.Price} (prepagato 3 mesi)");
            Console.WriteLine($"   - Premium 6 Months: €{premium6Months.Price} (prepagato 6 mesi)");
            Console.WriteLine($"   - Premium 12 Months: €{premium12Months.Price} (prepagato 12 mesi)");
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

