using BackEnd.Data;
using BackEnd.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd
{
    /// <summary>
    /// Script per popolare il database con dati di test per verificare le regole di accesso
    /// Uso: Chiamare SeedTestData() al bootstrap dell'applicazione in modalità Development
    /// </summary>
    public class TestDataSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TestDataSeeder(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedTestData()
        {
            Console.WriteLine("=== INIZIO SEED DATI TEST ===");

            // Pulisci dati esistenti (solo per test - commentare in produzione!)
            await CleanupTestData();

            // 1. CREAZIONE ADMIN
            var admin1 = await CreateAdmin("Mario", "Rossi", "mario.rossi@test.com");
            var admin2 = await CreateAdmin("Luigi", "Bianchi", "luigi.bianchi@test.com");

            Console.WriteLine($"\n✅ Creati 2 Admin:");
            Console.WriteLine($"   - {admin1.FirstName} {admin1.LastName} (ID: {admin1.Id})");
            Console.WriteLine($"   - {admin2.FirstName} {admin2.LastName} (ID: {admin2.Id})");

            // 2. CREAZIONE AGENCIES
            var agency1Admin1 = await CreateAgency("Agenzia_Mario", "Verdi", "agenzia.mario@test.com", admin1.Id);
            var agency2Admin1 = await CreateAgency("Agenzia_Mario_2", "Gialli", "agenzia2.mario@test.com", admin1.Id);
            var agency1Admin2 = await CreateAgency("Agenzia_Luigi", "Blu", "agenzia.luigi@test.com", admin2.Id);

            Console.WriteLine($"\n✅ Create 3 Agencies:");
            Console.WriteLine($"   - {agency1Admin1.FirstName} {agency1Admin1.LastName} (sotto Admin Mario)");
            Console.WriteLine($"   - {agency2Admin1.FirstName} {agency2Admin1.LastName} (sotto Admin Mario)");
            Console.WriteLine($"   - {agency1Admin2.FirstName} {agency1Admin2.LastName} (sotto Admin Luigi)");

            // 3. CREAZIONE AGENTS
            // Agent diretti di Admin1
            var agentDirettoMario1 = await CreateAgent("Agente_DirettoMario", "Uno", "agente.diretto.mario1@test.com", admin1.Id);
            var agentDirettoMario2 = await CreateAgent("Agente_DirettoMario", "Due", "agente.diretto.mario2@test.com", admin1.Id);

            // Agent dell'Agency1 di Mario
            var agentAgency1_1 = await CreateAgent("Agente_AgenziaVerdi", "Uno", "agente.verdi1@test.com", agency1Admin1.Id);
            var agentAgency1_2 = await CreateAgent("Agente_AgenziaVerdi", "Due", "agente.verdi2@test.com", agency1Admin1.Id);

            // Agent dell'Agency2 di Mario
            var agentAgency2_1 = await CreateAgent("Agente_AgenziaGialli", "Uno", "agente.gialli1@test.com", agency2Admin1.Id);

            // Agent diretti di Admin2
            var agentDirettoLuigi1 = await CreateAgent("Agente_DirettoLuigi", "Uno", "agente.diretto.luigi1@test.com", admin2.Id);

            // Agent dell'Agency di Luigi
            var agentAgencyLuigi1 = await CreateAgent("Agente_AgenziaBlu", "Uno", "agente.blu1@test.com", agency1Admin2.Id);

            Console.WriteLine($"\n✅ Creati 7 Agents:");
            Console.WriteLine($"   CERCHIA MARIO:");
            Console.WriteLine($"   - {agentDirettoMario1.FirstName} {agentDirettoMario1.LastName} (diretto di Admin Mario)");
            Console.WriteLine($"   - {agentDirettoMario2.FirstName} {agentDirettoMario2.LastName} (diretto di Admin Mario)");
            Console.WriteLine($"   - {agentAgency1_1.FirstName} {agentAgency1_1.LastName} (sotto Agenzia Verdi)");
            Console.WriteLine($"   - {agentAgency1_2.FirstName} {agentAgency1_2.LastName} (sotto Agenzia Verdi)");
            Console.WriteLine($"   - {agentAgency2_1.FirstName} {agentAgency2_1.LastName} (sotto Agenzia Gialli)");
            Console.WriteLine($"   CERCHIA LUIGI:");
            Console.WriteLine($"   - {agentDirettoLuigi1.FirstName} {agentDirettoLuigi1.LastName} (diretto di Admin Luigi)");
            Console.WriteLine($"   - {agentAgencyLuigi1.FirstName} {agentAgencyLuigi1.LastName} (sotto Agenzia Blu)");

            // 4. CREAZIONE CUSTOMERS
            var customerMario = await CreateCustomer("Cliente_Mario", "Uno", "cliente.mario@test.com", admin1.Id);
            var customerAgency1 = await CreateCustomer("Cliente_AgenziaVerdi", "Uno", "cliente.verdi@test.com", agency1Admin1.Id);
            var customerAgentVerdi1 = await CreateCustomer("Cliente_AgenteVerdi1", "Uno", "cliente.agenteverdi1@test.com", agentAgency1_1.Id);
            var customerAgentDirettoMario = await CreateCustomer("Cliente_AgenteDirettoMario", "Uno", "cliente.diretto.mario@test.com", agentDirettoMario1.Id);
            var customerLuigi = await CreateCustomer("Cliente_Luigi", "Uno", "cliente.luigi@test.com", admin2.Id);

            Console.WriteLine($"\n✅ Creati 5 Customers:");
            Console.WriteLine($"   CERCHIA MARIO:");
            Console.WriteLine($"   - {customerMario.FirstName} {customerMario.LastName} (creato da Admin Mario)");
            Console.WriteLine($"   - {customerAgency1.FirstName} {customerAgency1.LastName} (creato da Agency Verdi)");
            Console.WriteLine($"   - {customerAgentVerdi1.FirstName} {customerAgentVerdi1.LastName} (creato da Agente Verdi 1)");
            Console.WriteLine($"   - {customerAgentDirettoMario.FirstName} {customerAgentDirettoMario.LastName} (creato da Agente Diretto Mario 1)");
            Console.WriteLine($"   CERCHIA LUIGI:");
            Console.WriteLine($"   - {customerLuigi.FirstName} {customerLuigi.LastName} (creato da Admin Luigi)");

            // 5. CREAZIONE PROPERTIES (associate ai customers)
            var propertyMario = await CreateProperty("Appartamento di Mario Rossi", "Roma", 150000, customerMario.Id, admin1.Id);
            var propertyAgency1 = await CreateProperty("Villa dell'Agenzia Verdi", "Milano", 350000, customerAgency1.Id, agency1Admin1.Id);
            var propertyAgentVerdi1 = await CreateProperty("Monolocale di Agente Verdi 1", "Firenze", 80000, customerAgentVerdi1.Id, agentAgency1_1.Id);
            var propertyAgentDirettoMario = await CreateProperty("Casa di Agente Diretto Mario", "Napoli", 200000, customerAgentDirettoMario.Id, agentDirettoMario1.Id);
            var propertyLuigi = await CreateProperty("Ufficio di Luigi Bianchi", "Torino", 180000, customerLuigi.Id, admin2.Id);

            Console.WriteLine($"\n✅ Create 5 Properties:");
            Console.WriteLine($"   CERCHIA MARIO:");
            Console.WriteLine($"   - {propertyMario.Title} (creata da Admin Mario) - €{propertyMario.Price:N0}");
            Console.WriteLine($"   - {propertyAgency1.Title} (creata da Agency Verdi) - €{propertyAgency1.Price:N0}");
            Console.WriteLine($"   - {propertyAgentVerdi1.Title} (creata da Agente Verdi 1) - €{propertyAgentVerdi1.Price:N0}");
            Console.WriteLine($"   - {propertyAgentDirettoMario.Title} (creata da Agente Diretto Mario) - €{propertyAgentDirettoMario.Price:N0}");
            Console.WriteLine($"   CERCHIA LUIGI:");
            Console.WriteLine($"   - {propertyLuigi.Title} (creata da Admin Luigi) - €{propertyLuigi.Price:N0}");

            // 6. CREAZIONE REQUESTS
            var requestMario = await CreateRequest("Cerca villa mare", "Vendita", customerMario.Id, admin1.Id);
            var requestAgency1 = await CreateRequest("Cerca appartamento centro", "Affitto", customerAgency1.Id, agency1Admin1.Id);
            var requestAgentVerdi1 = await CreateRequest("Cerca monolocale studenti", "Affitto", customerAgentVerdi1.Id, agentAgency1_1.Id);
            var requestLuigi = await CreateRequest("Cerca ufficio zona industriale", "Vendita", customerLuigi.Id, admin2.Id);

            Console.WriteLine($"\n✅ Create 4 Requests:");
            Console.WriteLine($"   CERCHIA MARIO:");
            Console.WriteLine($"   - {requestMario.Notes} (creata da Admin Mario)");
            Console.WriteLine($"   - {requestAgency1.Notes} (creata da Agency Verdi)");
            Console.WriteLine($"   - {requestAgentVerdi1.Notes} (creata da Agente Verdi 1)");
            Console.WriteLine($"   CERCHIA LUIGI:");
            Console.WriteLine($"   - {requestLuigi.Notes} (creata da Admin Luigi)");

            // 7. CREAZIONE EVENTI CALENDARIO
            await CreateCalendarEvent("Appuntamento Mario", admin1.Id, propertyMario.Id, customerMario.Id);
            await CreateCalendarEvent("Visita Agenzia Verdi", agentAgency1_1.Id, propertyAgency1.Id, customerAgency1.Id);
            await CreateCalendarEvent("Incontro Cliente Luigi", admin2.Id, propertyLuigi.Id, customerLuigi.Id);

            Console.WriteLine($"\n✅ Creati 3 Eventi Calendario");

            Console.WriteLine("\n=== SEED COMPLETATO CON SUCCESSO ===");
            Console.WriteLine("\n📊 RIEPILOGO STRUTTURA:");
            Console.WriteLine("┌─────────────────────────────────────────────────┐");
            Console.WriteLine("│ CERCHIA ADMIN MARIO (mario.rossi@test.com)     │");
            Console.WriteLine("├─────────────────────────────────────────────────┤");
            Console.WriteLine("│ └─ Agency: Agenzia_Mario Verdi                  │");
            Console.WriteLine("│    └─ Agent: Agente_AgenziaVerdi Uno            │");
            Console.WriteLine("│    └─ Agent: Agente_AgenziaVerdi Due            │");
            Console.WriteLine("│ └─ Agency: Agenzia_Mario_2 Gialli               │");
            Console.WriteLine("│    └─ Agent: Agente_AgenziaGialli Uno           │");
            Console.WriteLine("│ └─ Agent Diretto: Agente_DirettoMario Uno       │");
            Console.WriteLine("│ └─ Agent Diretto: Agente_DirettoMario Due       │");
            Console.WriteLine("└─────────────────────────────────────────────────┘");
            Console.WriteLine("┌─────────────────────────────────────────────────┐");
            Console.WriteLine("│ CERCHIA ADMIN LUIGI (luigi.bianchi@test.com)   │");
            Console.WriteLine("├─────────────────────────────────────────────────┤");
            Console.WriteLine("│ └─ Agency: Agenzia_Luigi Blu                    │");
            Console.WriteLine("│    └─ Agent: Agente_AgenziaBlu Uno              │");
            Console.WriteLine("│ └─ Agent Diretto: Agente_DirettoLuigi Uno       │");
            Console.WriteLine("└─────────────────────────────────────────────────┘");
            Console.WriteLine("\n📝 CREDENZIALI DI ACCESSO (password per tutti: Test@1234):");
            Console.WriteLine($"   Admin Mario:  mario.rossi@test.com");
            Console.WriteLine($"   Admin Luigi:  luigi.bianchi@test.com");
            Console.WriteLine($"   Agency Verdi: agenzia.mario@test.com");
            Console.WriteLine($"   Agent Verdi1: agente.verdi1@test.com");
            Console.WriteLine("\n🧪 SCENARI DI TEST SUGGERITI:");
            Console.WriteLine("   1. Login come Admin Mario → Vedi tutti nella tua cerchia (5 agents)");
            Console.WriteLine("   2. Login come Agency Verdi → Vedi solo i tuoi 2 agents");
            Console.WriteLine("   3. Login come Agent Verdi 1 → Vedi i colleghi (Agent Verdi 2)");
            Console.WriteLine("   4. Prova a modificare Property di Luigi da account Mario → ❌ 403");
            Console.WriteLine("   5. Prova ad associare Customer di Luigi in Calendar di Mario → ❌ 403");
            Console.WriteLine("   6. Agent Verdi 1 prova a modificare Property di Agent Verdi 2 → ❌ 403");
            Console.WriteLine("   7. Agency Verdi modifica Property di Agent Verdi 1 → ✅ OK");
            Console.WriteLine("   8. Admin Mario modifica qualsiasi dato della sua cerchia → ✅ OK");
        }

        private async Task CleanupTestData()
        {
            Console.WriteLine("🧹 Pulizia dati test esistenti...");

            // Elimina eventi calendario
            var events = await _context.Calendars.Where(c => c.User.Email.Contains("@test.com")).ToListAsync();
            _context.Calendars.RemoveRange(events);

            // Elimina requests
            var requests = await _context.Requests.Where(r => r.User.Email.Contains("@test.com")).ToListAsync();
            _context.Requests.RemoveRange(requests);

            // Elimina properties
            var properties = await _context.RealEstateProperties.Where(p => p.User.Email.Contains("@test.com")).ToListAsync();
            _context.RealEstateProperties.RemoveRange(properties);

            // Elimina customers
            var customers = await _context.Customers.Where(c => c.User.Email.Contains("@test.com")).ToListAsync();
            _context.Customers.RemoveRange(customers);

            // Elimina utenti test (questo eliminerà anche le relazioni per cascade)
            var testUsers = await _userManager.Users.Where(u => u.Email.Contains("@test.com")).ToListAsync();
            foreach (var user in testUsers)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Pulizia completata");
        }

        private async Task<ApplicationUser> CreateAdmin(string firstName, string lastName, string email)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = "1234567890",
                Address = "Via Test 123",
                City = "Roma",
                EmailConfirmed = true,
                AdminId = null,
                Color = "#3699FF"
            };

            var result = await _userManager.CreateAsync(user, "Test@1234");
            if (!result.Succeeded)
                throw new Exception($"Errore creazione Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "Admin");
            return user;
        }

        private async Task<ApplicationUser> CreateAgency(string firstName, string lastName, string email, string adminId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = "1234567890",
                Address = "Via Test 123",
                City = "Milano",
                EmailConfirmed = true,
                AdminId = adminId,
                Color = "#1BC5BD"
            };

            var result = await _userManager.CreateAsync(user, "Test@1234");
            if (!result.Succeeded)
                throw new Exception($"Errore creazione Agency: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "Agency");
            return user;
        }

        private async Task<ApplicationUser> CreateAgent(string firstName, string lastName, string email, string agencyId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = "1234567890",
                Address = "Via Test 123",
                City = "Firenze",
                EmailConfirmed = true,
                AdminId = agencyId,
                Color = "#8950FC"
            };

            var result = await _userManager.CreateAsync(user, "Test@1234");
            if (!result.Succeeded)
                throw new Exception($"Errore creazione Agent: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "Agent");
            return user;
        }

        private async Task<Customer> CreateCustomer(string firstName, string lastName, string email, string userId)
        {
            var customer = new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = 1234567890,
                Address = "Via Cliente 123",
                City = "Roma",
                State = "RM",
                Buyer = true,
                Seller = false,
                Builder = false,
                Other = false,
                GoldCustomer = false,
                Description = $"Cliente di test creato da {userId}",
                UserId = userId,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                AcquisitionDone = false,
                OngoingAssignment = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        private async Task<RealEstateProperty> CreateProperty(string title, string city, double price, int customerId, string userId)
        {
            var property = new RealEstateProperty
            {
                Title = title,
                Category = "Residenziale",
                Typology = "Appartamento",
                Status = "Disponibile",
                AddressLine = "Via Immobile 123",
                City = city,
                State = "RM",
                Location = "Centro",
                PostCode = "00100",
                CommercialSurfaceate = 100,
                Bedrooms = 2,
                Bathrooms = 1,
                Kitchens = 1,
                Price = price,
                CustomerId = customerId, // Associa al customer
                TypeOfProperty = "Abitativo",
                StateOfTheProperty = "Ottimo",
                YearOfConstruction = 2020,
                Description = $"Immobile di test creato da {userId}",
                UserId = userId,
                InHome = false,
                Highlighted = false,
                Auction = false,
                Negotiation = false,
                Sold = false,
                Archived = false,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                AssignmentEnd = DateTime.UtcNow.AddYears(1),
                TypeOfAssignment = "Vendita",
                AgreedCommission = 3,
                FlatRateCommission = 0,
                CommissionReversal = 0
            };

            _context.RealEstateProperties.Add(property);
            await _context.SaveChangesAsync();
            return property;
        }

        private async Task<Request> CreateRequest(string notes, string contract, int customerId, string userId)
        {
            var request = new Request
            {
                CustomerId = customerId,
                Contract = contract,
                PropertyType = "Appartamento",
                Province = "RM",
                City = "Roma",
                Location = "Centro",
                PriceFrom = 100000,
                PriceTo = 200000,
                MQFrom = 50,
                MQTo = 100,
                RoomsNumber = "2-3",
                Notes = notes,
                Archived = false,
                Closed = false,
                UserId = userId,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                MortgageAdviceRequired = false
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        private async Task<Calendar> CreateCalendarEvent(string eventName, string userId, int? propertyId, int? customerId)
        {
            var calendarEvent = new Calendar
            {
                UserId = userId,
                EventName = eventName,
                Type = "Appuntamento",
                EventDescription = $"Evento di test per {eventName}",
                EventLocation = "Ufficio",
                EventStartDate = DateTime.UtcNow.AddDays(1),
                EventEndDate = DateTime.UtcNow.AddDays(1).AddHours(1),
                RealEstatePropertyId = propertyId,
                CustomerId = customerId,
                Color = "#3699FF",
                Confirmed = false,
                Cancelled = false,
                Postponed = false,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            _context.Calendars.Add(calendarEvent);
            await _context.SaveChangesAsync();
            return calendarEvent;
        }
    }
}

