using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.OutputModels;
using BackEnd.Models.UserModel;
using BackEnd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace BackEnd.Services.BusinessServices
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly AccessControlService _accessControl;
        private const int CACHE_DURATION_MINUTES = 5;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DashboardService> logger,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            AccessControlService accessControl)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _cache = cache;
            _accessControl = accessControl;
        }

        public async Task<MapDataModel> GetMapData(string? userId, string? agencyId, int? year)
        {
            try
            {
                // Genera chiave cache basata su userId, agencyId e year
                var cacheKey = $"MapData_{userId}_{agencyId ?? "all"}_{year ?? DateTime.UtcNow.Year}";

                // Verifica se i dati sono in cache
                if (_cache.TryGetValue(cacheKey, out MapDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati mappa recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new MapDataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;

                // Determina l'adminId in base al ruolo dell'utente
                ApplicationUser? currentUser = null;
                string? adminId = null;

                if (!string.IsNullOrEmpty(userId))
                {
                    currentUser = await _userManager.FindByIdAsync(userId);
                    if (currentUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(currentUser);
                        
                        if (roles.Contains("Admin"))
                        {
                            // Admin: usa il proprio ID come adminId
                            adminId = currentUser.Id;
                        }
                        else if (roles.Contains("Agency"))
                        {
                            // Agency: usa il proprio AdminId (se esiste) o il proprio ID
                            adminId = currentUser.AdminId ?? currentUser.Id;
                        }
                        else if (roles.Contains("Agent"))
                        {
                            // Agent: usa il proprio AdminId
                            adminId = currentUser.AdminId;
                        }
                    }
                }

                // Se è specificato un agencyId nel filtro, usa quello (per filtrare per agenzia/agente specifico)
                string? filterAgencyId = null;
                if (!string.IsNullOrEmpty(agencyId))
                {
                    if (agencyId.StartsWith("agency_"))
                    {
                        filterAgencyId = agencyId.Replace("agency_", "");
                    }
                    else if (agencyId.StartsWith("agent_"))
                    {
                        filterAgencyId = agencyId.Replace("agent_", "");
                    }
                }

                // ===== AGENCIES =====
                // Se è Admin, mostra tutte le agenzie della sua cerchia
                // Se è Agency, mostra solo se stessa
                // Se è Agent, mostra la sua agenzia
                // Recupera tutte le agenzie usando GetUsersInRoleAsync
                var allAgencies = await _userManager.GetUsersInRoleAsync("Agency");
                IQueryable<ApplicationUser> agenciesBaseQuery = _unitOfWork.dbContext.Users
                    .Where(u => allAgencies.Select(a => a.Id).Contains(u.Id));

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Admin: mostra tutte le agenzie della sua cerchia + se stesso
                        agenciesBaseQuery = agenciesBaseQuery.Where(u => u.AdminId == adminId || u.Id == adminId);
                    }
                    else if (roles.Contains("Agency"))
                    {
                        // Agency: mostra solo se stessa
                        agenciesBaseQuery = agenciesBaseQuery.Where(u => u.Id == currentUser.Id);
                    }
                    else if (roles.Contains("Agent"))
                    {
                        // Agent: mostra la sua agenzia (se esiste)
                        if (!string.IsNullOrEmpty(currentUser.AdminId))
                        {
                            agenciesBaseQuery = agenciesBaseQuery.Where(u => u.Id == currentUser.AdminId);
                        }
                        else
                        {
                            // Nessuna agenzia se l'agent non ha AdminId
                            agenciesBaseQuery = agenciesBaseQuery.Where(u => false);
                        }
                    }
                }

                // Applica filtro agencyId se specificato (per la lista visualizzata sulla mappa)
                IQueryable<ApplicationUser> agenciesQuery = agenciesBaseQuery;
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    if (agencyId!.StartsWith("agency_"))
                    {
                        // Verifica che l'agenzia del filtro appartenga alla cerchia dell'utente
                        var filteredAgencyId = filterAgencyId;
                        var isInCircle = agenciesBaseQuery.Any(u => u.Id == filteredAgencyId);
                        
                        if (isInCircle)
                        {
                            // Filtra per agenzia specifica
                            agenciesQuery = agenciesQuery.Where(u => u.Id == filteredAgencyId);
                        }
                        else
                        {
                            // Agenzia non nella cerchia, non mostrare nulla
                            agenciesQuery = agenciesQuery.Where(u => false);
                        }
                    }
                    else if (agencyId.StartsWith("agent_"))
                    {
                        // Verifica che l'agente appartenga alla cerchia prima di filtrare
                        var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId ?? "");
                        if (circleUserIds.Contains(filterAgencyId))
                        {
                            // Filtra per agenzia dell'agente specificato
                            var agent = await _userManager.FindByIdAsync(filterAgencyId);
                            if (agent != null && !string.IsNullOrEmpty(agent.AdminId))
                            {
                                agenciesQuery = agenciesQuery.Where(u => u.Id == agent.AdminId);
                            }
                            else
                            {
                                agenciesQuery = agenciesQuery.Where(u => false);
                            }
                        }
                        else
                        {
                            // Agente non nella cerchia, non mostrare nulla
                            agenciesQuery = agenciesQuery.Where(u => false);
                        }
                    }
                }

                var agenciesList = await agenciesQuery.ToListAsync();
                
                // Mappa solo i campi necessari per la mappa (modello leggero)
                result.Agencies = agenciesList.Select(a => new MapAgencyModel
                {
                    Id = a.Id,
                    UserName = a.UserName ?? string.Empty,
                    AdminId = a.AdminId,
                    Address = a.Address,
                    City = a.City,
                    Province = a.Province,
                    ZipCode = a.ZipCode,
                    PhoneNumber = a.PhoneNumber,
                    Email = a.Email
                }).ToList();

                // Aggiungi l'admin stesso se è Admin e non è già nella lista (per la visualizzazione)
                // MA NON aggiungerlo se c'è un filtro attivo per agenzia/agente specifico
                if (currentUser != null && string.IsNullOrEmpty(filterAgencyId))
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin") && !result.Agencies.Any(a => a.Id == currentUser.Id))
                    {
                        result.Agencies.Add(new MapAgencyModel
                        {
                            Id = currentUser.Id,
                            UserName = currentUser.UserName ?? string.Empty,
                            AdminId = currentUser.AdminId,
                            Address = currentUser.Address,
                            City = currentUser.City,
                            Province = currentUser.Province,
                            ZipCode = currentUser.ZipCode,
                            PhoneNumber = currentUser.PhoneNumber,
                            Email = currentUser.Email
                        });
                    }
                }

                // Calcola i totali DOPO aver applicato il filtro
                result.TotalAgencies = result.Agencies.Count;

                // ===== AGENTS =====
                // Recupera tutti gli agenti
                var allAgents = await _userManager.GetUsersInRoleAsync("Agent");
                IQueryable<ApplicationUser> agentsBaseQuery = _unitOfWork.dbContext.Users
                    .Where(u => allAgents.Select(a => a.Id).Contains(u.Id));

                // Applica filtri in base al ruolo dell'utente
                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Admin: vede i propri Agent + Agent delle proprie Agency
                        // Usa GetCircleUserIdsFor per ottenere la cerchia completa
                        var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId ?? "");
                        agentsBaseQuery = agentsBaseQuery.Where(u => circleUserIds.Contains(u.Id));
                    }
                    else if (roles.Contains("Agency"))
                    {
                        // Agency: vede SOLO i propri Agent (dove AdminId == Agency.Id)
                        // IMPORTANTE: non include agenti con AdminId == Admin.Id
                        agentsBaseQuery = agentsBaseQuery.Where(u => u.AdminId == currentUser.Id);
                    }
                    else if (roles.Contains("Agent"))
                    {
                        // Agent: vede solo i colleghi (stessa Agency o stesso Admin)
                        var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId ?? "");
                        agentsBaseQuery = agentsBaseQuery.Where(u => circleUserIds.Contains(u.Id));
                    }
                }

                // Applica filtro agencyId se specificato (per la lista visualizzata nel dropdown)
                IQueryable<ApplicationUser> agentsQuery = agentsBaseQuery;
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    if (agencyId!.StartsWith("agency_"))
                    {
                        // Filtra per agenti dell'agenzia specifica
                        agentsQuery = agentsQuery.Where(u => u.AdminId == filterAgencyId);
                    }
                    else if (agencyId.StartsWith("agent_"))
                    {
                        // Filtra per agente specifico
                        agentsQuery = agentsQuery.Where(u => u.Id == filterAgencyId);
                    }
                }

                var agentsList = await agentsQuery.ToListAsync();
                
                // Mappa solo i campi necessari per il dropdown filtro (modello leggero)
                result.Agents = agentsList.Select(a => new MapAgentModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).ToList();

                // Calcola i totali DOPO aver applicato il filtro
                result.TotalAgents = result.Agents.Count;


                // Salva in cache per 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati mappa calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei dati della mappa");
                throw new Exception("Si è verificato un errore nel recupero dei dati della mappa");
            }
        }

        public async Task<Widget3DataModel> GetWidget3Data(string? userId, string? agencyId, int? year)
        {
            try
            {
                // Genera chiave cache basata su userId, agencyId e year
                var cacheKey = $"Widget3Data_{userId}_{agencyId ?? "all"}_{year ?? DateTime.UtcNow.Year}";

                // Verifica se i dati sono in cache
                if (_cache.TryGetValue(cacheKey, out Widget3DataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Widget3 recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData;
                }

                var result = new Widget3DataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"UserId non specificato per GetWidget3Data");
                    return result;
                }

                // Ottieni tutti gli userId nella cerchia dell'utente corrente
                // GetCircleUserIdsFor gestisce automaticamente i ruoli:
                // - Admin: restituisce Admin + Agency + Agent
                // - Agency: restituisce Agency + Agent
                // - Agent: restituisce Agent + colleghi
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);

                // Parse filterAgencyId se presente
                string? filterAgencyId = null;
                if (!string.IsNullOrEmpty(agencyId) && agencyId != "all")
                {
                    filterAgencyId = agencyId;
                }

                // Query base per gli immobili nella cerchia dell'utente corrente
                // Include User per accedere a AdminId (usato nel filtro agency)
                // GetCircleUserIdsFor già restituisce tutti gli ID nella cerchia corretta in base al ruolo
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => circleUserIds.Contains(p.UserId));

                // Applica filtro agencyId se specificato
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    if (filterAgencyId.StartsWith("agency_"))
                    {
                        var agencyIdOnly = filterAgencyId.Replace("agency_", "");
                        propertiesQuery = propertiesQuery.Where(p => 
                            (p.User != null && p.User.AdminId == agencyIdOnly) ||
                            (p.UserId == agencyIdOnly));
                    }
                    else if (filterAgencyId.StartsWith("agent_"))
                    {
                        var agentIdOnly = filterAgencyId.Replace("agent_", "");
                        propertiesQuery = propertiesQuery.Where(p => p.UserId == agentIdOnly);
                    }
                    else
                    {
                        // Se non c'è prefisso, determina automaticamente se è agenzia o agente
                        // Cerca prima se è un'agenzia, altrimenti filtra come agente
                        var allAgencies = await _userManager.GetUsersInRoleAsync("Agency");
                        var isAgency = allAgencies.Any(a => a.Id == filterAgencyId);
                        
                        if (isAgency)
                        {
                            // È un'agenzia: filtra per immobili dell'agenzia o dell'admin dell'agenzia
                            propertiesQuery = propertiesQuery.Where(p => 
                                (p.User != null && p.User.AdminId == filterAgencyId) ||
                                (p.UserId == filterAgencyId));
                        }
                        else
                        {
                            // È un agente: filtra solo per immobili dell'agente
                            propertiesQuery = propertiesQuery.Where(p => p.UserId == filterAgencyId);
                        }
                    }
                }

                // Filtra per anno (CreationDate o UpdateDate nell'anno)
                // Include anche immobili venduti creati nell'anno corrente (anche se UpdateDate non è impostato)
                // perché potrebbero essere stati venduti nell'anno corrente
                var startYear = new DateTime(currentYear, 1, 1);
                var endYear = new DateTime(currentYear, 12, 31, 23, 59, 59);

                var allProperties = await propertiesQuery
                    .Where(p => (p.CreationDate.Year == currentYear) || 
                               (p.UpdateDate.Year == currentYear && p.UpdateDate != default(DateTime)) ||
                               // Include immobili venduti creati nell'anno corrente (potrebbero essere stati venduti nell'anno)
                               (p.Sold && p.CreationDate.Year == currentYear))
                    .ToListAsync();

                // Genera array dei 12 mesi dell'anno
                // Usa formato compatibile con frontend: "gen 25" (senza punto)
                var months = new List<string>();
                var culture = new System.Globalization.CultureInfo("it-IT");
                for (int i = 0; i < 12; i++)
                {
                    var date = new DateTime(currentYear, i + 1, 1);
                    // Formato: "gen 25" (rimuove eventuali punti dal formato standard)
                    var monthKey = date.ToString("MMM yy", culture).Replace(".", "").Trim();
                    months.Add(monthKey);
                }
                result.Months = months;
                
                _logger.LogInformation($"Mesi generati per anno {currentYear}: {string.Join(", ", months)}");

                // Inizializza i dizionari per ogni mese
                var propertiesData = new PropertiesDataModel();
                var soldPropertiesData = new PropertiesDataModel();
                var commissionsMonthly = new Dictionary<string, decimal>();

                foreach (var month in months)
                {
                    propertiesData.Sale[month] = 0;
                    propertiesData.Rent[month] = 0;
                    propertiesData.Auction[month] = 0;
                    soldPropertiesData.Sale[month] = 0;
                    soldPropertiesData.Rent[month] = 0;
                    soldPropertiesData.Auction[month] = 0;
                    commissionsMonthly[month] = 0;
                }

                // Processa ogni immobile
                foreach (var property in allProperties)
                {
                    // Data per immobili inseriti (usa CreationDate)
                    var creationDate = property.CreationDate;
                    var creationMonth = creationDate.ToString("MMM yy", new System.Globalization.CultureInfo("it-IT")).Replace(".", "").Trim();

                    if (creationDate.Year == currentYear && months.Contains(creationMonth))
                    {
                        // Verifica che l'incarico non sia scaduto
                        bool isAssignmentValid = false;
                        if (property.AssignmentEnd == default(DateTime) || property.AssignmentEnd == new DateTime(1, 1, 1))
                        {
                            // Incarico senza scadenza, sempre valido
                            isAssignmentValid = true;
                        }
                        else
                        {
                            // L'incarico deve essere valido (non scaduto)
                            isAssignmentValid = property.AssignmentEnd > now;
                        }

                        // Categorizza per Status e Auction solo se l'incarico è valido
                        if (isAssignmentValid)
                        {
                            if (property.Auction)
                            {
                                propertiesData.Auction[creationMonth]++;
                            }
                            else if (property.Status == "Vendita")
                            {
                                propertiesData.Sale[creationMonth]++;
                            }
                            else if (property.Status == "Affitto")
                            {
                                propertiesData.Rent[creationMonth]++;
                            }
                        }
                    }

                    // Data per immobili venduti (usa UpdateDate se valida, altrimenti CreationDate)
                    if (property.Sold)
                    {
                        DateTime soldDate;
                        if (property.UpdateDate != default(DateTime) && property.UpdateDate != new DateTime(1, 1, 1))
                        {
                            soldDate = property.UpdateDate;
                        }
                        else
                        {
                            soldDate = property.CreationDate;
                        }

                        var soldMonth = soldDate.ToString("MMM yy", new System.Globalization.CultureInfo("it-IT")).Replace(".", "").Trim();

                        if (soldDate.Year == currentYear && months.Contains(soldMonth))
                        {
                            // Verifica che l'incarico non sia scaduto al momento della vendita
                            bool isAssignmentValid = false;
                            if (property.AssignmentEnd == default(DateTime) || property.AssignmentEnd == new DateTime(1, 1, 1))
                            {
                                // Incarico senza scadenza, sempre valido
                                isAssignmentValid = true;
                            }
                            else
                            {
                                // L'incarico deve essere valido al momento della vendita
                                isAssignmentValid = property.AssignmentEnd > soldDate || property.AssignmentEnd > now;
                            }

                            // Categorizza per Status e Auction (solo se l'incarico è valido)
                            if (isAssignmentValid)
                            {
                                if (property.Auction)
                                {
                                    soldPropertiesData.Auction[soldMonth]++;
                                }
                                else if (property.Status == "Vendita")
                                {
                                    soldPropertiesData.Sale[soldMonth]++;
                                }
                                else if (property.Status == "Affitto")
                                {
                                    soldPropertiesData.Rent[soldMonth]++;
                                }

                                // Aggiungi provvigione al mese di vendita solo se l'incarico è valido
                                var commission = (decimal)property.EffectiveCommission;
                                if (commission > 0)
                                {
                                    _logger.LogInformation($"Immobile {property.Id}: EffectiveCommission={commission}, soldMonth={soldMonth}, soldDate={soldDate:yyyy-MM-dd}, AssignmentEnd={property.AssignmentEnd:yyyy-MM-dd}");
                                }
                                commissionsMonthly[soldMonth] += commission;
                            }
                            else
                            {
                                // Immobile venduto ma con incarico scaduto, non includere nelle statistiche
                                _logger.LogWarning($"Immobile {property.Id} venduto ma incarico scaduto: soldDate={soldDate:yyyy-MM-dd}, AssignmentEnd={property.AssignmentEnd:yyyy-MM-dd}");
                            }
                        }
                        else
                        {
                            // Log per debug se non viene incluso
                            var commission = (decimal)property.EffectiveCommission;
                            if (commission > 0)
                            {
                                _logger.LogWarning($"Immobile {property.Id} venduto con provvigione {commission} ma non incluso: soldDate.Year={soldDate.Year}, currentYear={currentYear}, soldMonth={soldMonth}, months.Contains={months.Contains(soldMonth)}");
                            }
                        }
                    }
                }

                result.PropertiesData = propertiesData;
                result.SoldPropertiesData = soldPropertiesData;
                result.CommissionsMonthlyData = commissionsMonthly;

                // Log per debug provvigioni
                var commissionsSummary = string.Join(", ", commissionsMonthly.Where(kvp => kvp.Value > 0).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                _logger.LogInformation($"Provvigioni mensili (solo > 0): {commissionsSummary}");
                _logger.LogInformation($"Totale immobili processati: {allProperties.Count}, Venduti: {allProperties.Count(p => p.Sold)}");
                
                // Log dettagliato per immobili venduti
                var soldProperties = allProperties.Where(p => p.Sold).ToList();
                _logger.LogInformation($"Immobili venduti trovati: {soldProperties.Count}");
                foreach (var soldProp in soldProperties)
                {
                    var soldDate = (soldProp.UpdateDate != default(DateTime) && soldProp.UpdateDate != new DateTime(1, 1, 1))
                        ? soldProp.UpdateDate
                        : soldProp.CreationDate;
                    var isInYear = soldDate.Year == currentYear;
                    var isAssignmentValid = soldProp.AssignmentEnd == default(DateTime) || soldProp.AssignmentEnd == new DateTime(1, 1, 1) 
                        ? true 
                        : (soldProp.AssignmentEnd > soldDate || soldProp.AssignmentEnd > now);
                    _logger.LogInformation($"Immobile {soldProp.Id}: Sold={soldProp.Sold}, EffectiveCommission={soldProp.EffectiveCommission}, " +
                        $"CreationDate={soldProp.CreationDate:yyyy-MM-dd}, UpdateDate={soldProp.UpdateDate:yyyy-MM-dd}, " +
                        $"soldDate={soldDate:yyyy-MM-dd}, soldDate.Year={soldDate.Year}, currentYear={currentYear}, " +
                        $"isInYear={isInYear}, AssignmentEnd={soldProp.AssignmentEnd:yyyy-MM-dd}, isAssignmentValid={isAssignmentValid}");
                }
                
                _logger.LogInformation($"TotalCommissionsPortfolio: {result.TotalCommissionsPortfolio}, TotalCommissionsEarned: {result.TotalCommissionsEarned}");

                // Calcola totali provvigioni
                // Portafoglio = somma EffectiveCommission degli immobili:
                // - Sold = false (non venduti)
                // - AssignmentEnd > oggi (incarico non scaduto) o AssignmentEnd è default/null (incarico senza scadenza)
                // - CreationDate nell'anno selezionato
                result.TotalCommissionsPortfolio = allProperties
                    .Where(p => !p.Sold) // Solo immobili non venduti
                    .Where(p => p.CreationDate.Year == currentYear) // Creati nell'anno selezionato
                    .Where(p => 
                    {
                        // Se AssignmentEnd è default o null, considera l'incarico come non scaduto
                        if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                        {
                            return true; // Incarico senza scadenza, sempre valido
                        }
                        // Altrimenti verifica che non sia scaduto
                        return p.AssignmentEnd > now;
                    })
                    .Sum(p => (decimal)p.EffectiveCommission);

                // Incassati = somma EffectiveCommission degli immobili venduti nell'anno
                // ma solo se l'incarico non è scaduto (AssignmentEnd > oggi o null)
                result.TotalCommissionsEarned = allProperties
                    .Where(p => p.Sold) // Solo immobili venduti
                    .Where(p =>
                    {
                        var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                            ? p.UpdateDate
                            : p.CreationDate;
                        return soldDate.Year == currentYear; // Venduti nell'anno selezionato
                    })
                    .Where(p =>
                    {
                        // Se AssignmentEnd è default o null, considera l'incarico come non scaduto
                        if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                        {
                            return true; // Incarico senza scadenza, sempre valido
                        }
                        // Verifica che l'incarico non sia scaduto al momento della vendita
                        var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                            ? p.UpdateDate
                            : p.CreationDate;
                        // L'incarico deve essere valido al momento della vendita
                        return p.AssignmentEnd > soldDate || p.AssignmentEnd > now;
                    })
                    .Sum(p => (decimal)p.EffectiveCommission);

                // Salva in cache per 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Widget3 calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei dati Widget3");
                throw new Exception("Si è verificato un errore nel recupero dei dati Widget3");
            }
        }

        public async Task<TopAgenciesDataModel> GetTopAgenciesData(string? userId, int? year, string? sortBy = null, string? sortOrder = "desc")
        {
            try
            {
                // Genera chiave cache basata su userId, year, sortBy e sortOrder
                var cacheKey = $"TopAgenciesData_{userId}_{year ?? DateTime.UtcNow.Year}_{sortBy ?? "SoldProperties"}_{sortOrder ?? "desc"}";

                // Verifica se i dati sono in cache
                if (_cache.TryGetValue(cacheKey, out TopAgenciesDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Top Agenzie recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new TopAgenciesDataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"UserId non specificato per GetTopAgenciesData");
                    return result;
                }

                // Determina l'adminId in base al ruolo dell'utente
                ApplicationUser? currentUser = await _userManager.FindByIdAsync(userId);
                string? adminId = null;

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    
                    if (roles.Contains("Admin"))
                    {
                        adminId = currentUser.Id;
                    }
                    else if (roles.Contains("Agency"))
                    {
                        adminId = currentUser.AdminId ?? currentUser.Id;
                    }
                    else if (roles.Contains("Agent"))
                    {
                        adminId = currentUser.AdminId;
                    }
                }

                // Recupera tutte le agenzie nella cerchia dell'utente
                var allAgencies = await _userManager.GetUsersInRoleAsync("Agency");
                IQueryable<ApplicationUser> agenciesQuery = _unitOfWork.dbContext.Users
                    .Where(u => allAgencies.Select(a => a.Id).Contains(u.Id));

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Admin: mostra tutte le agenzie della sua cerchia + se stesso
                        agenciesQuery = agenciesQuery.Where(u => u.AdminId == adminId || u.Id == adminId);
                    }
                    else if (roles.Contains("Agency"))
                    {
                        // Agency: mostra solo se stessa
                        agenciesQuery = agenciesQuery.Where(u => u.Id == currentUser.Id);
                    }
                    else if (roles.Contains("Agent"))
                    {
                        // Agent: mostra la sua agenzia (se esiste)
                        if (!string.IsNullOrEmpty(currentUser.AdminId))
                        {
                            agenciesQuery = agenciesQuery.Where(u => u.Id == currentUser.AdminId);
                        }
                        else
                        {
                            agenciesQuery = agenciesQuery.Where(u => false);
                        }
                    }
                }

                var agenciesList = await agenciesQuery.ToListAsync();

                // Aggiungi l'admin stesso se è Admin e non è già nella lista
                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin") && !agenciesList.Any(a => a.Id == currentUser.Id))
                    {
                        agenciesList.Add(currentUser);
                    }
                }

                // Ottieni tutti gli userId nella cerchia per filtrare immobili/appuntamenti/richieste
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Per ogni agenzia, calcola i dati aggregati
                foreach (var agency in agenciesList)
                {
                    var agencyId = agency.Id;
                    
                    // Recupera tutti gli agenti dell'agenzia
                    var agencyAgents = await _userManager.Users
                        .Where(u => u.AdminId == agencyId)
                        .ToListAsync();
                    
                    var agencyAgentIds = agencyAgents.Select(a => a.Id).ToList();

                    // Recupera immobili dell'agenzia (include anche immobili dell'admin se è l'agenzia admin)
                    var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   ((p.User != null && !string.IsNullOrEmpty(p.User.AdminId) && p.User.AdminId == agencyId) || p.UserId == agencyId));
                    
                    var allAgencyProperties = await propertiesQuery.ToListAsync();

                    // Properties: totale immobili dell'agenzia creati nell'anno
                    var properties = allAgencyProperties
                        .Where(p => p.CreationDate.Year == currentYear)
                        .Count();

                    // Customers: numero clienti degli agenti dell'agenzia creati nell'anno
                    var customers = await _unitOfWork.dbContext.Customers
                        .Where(c => (c.UserId != null && agencyAgentIds.Contains(c.UserId)) || c.UserId == agencyId)
                        .Where(c => c.CreationDate.Year == currentYear)
                        .CountAsync();

                    // SoldProperties: immobili venduti nell'anno
                    var soldProperties = allAgencyProperties
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year == currentYear;
                        })
                        .Count();

                    // Appointments: appuntamenti degli agenti dell'agenzia nell'anno
                    // Usa confronto per anno invece di date complete per evitare problemi UTC con PostgreSQL
                    var appointments = await _unitOfWork.dbContext.Calendars
                        .Where(c => agencyAgentIds.Contains(c.UserId) || c.UserId == agencyId)
                        .Where(c => c.EventStartDate.Year == currentYear)
                        .Where(c => !c.Cancelled)
                        .CountAsync();

                    // Requests: richieste degli agenti dell'agenzia create nell'anno
                    var requests = await _unitOfWork.dbContext.Requests
                        .Where(r => (r.UserId != null && agencyAgentIds.Contains(r.UserId)) || r.UserId == agencyId)
                        .Where(r => r.CreationDate.Year == currentYear)
                        .CountAsync();

                    // Commissions: guadagni totali (somma EffectiveCommission degli immobili venduti)
                    var commissions = allAgencyProperties
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year == currentYear;
                        })
                        .Sum(p => (decimal)p.EffectiveCommission);

                    // Costruisci location (City, Province)
                    var location = string.IsNullOrEmpty(agency.City) 
                        ? string.Empty 
                        : $"{agency.City}" + (string.IsNullOrEmpty(agency.Province) ? string.Empty : $", {agency.Province}");

                    result.Agencies.Add(new TopAgencyItemModel
                    {
                        Id = agencyId,
                        Name = agency.UserName ?? $"{agency.FirstName} {agency.LastName}".Trim() ?? "Agenzia",
                        Location = location,
                        Properties = properties,
                        Customers = customers,
                        Requests = requests,
                        SoldProperties = soldProperties,
                        Appointments = appointments,
                        Commissions = commissions
                    });
                }

                // Ordina dinamicamente in base al parametro sortBy
                var sortedAgencies = sortBy?.ToLower() switch
                {
                    "properties" => sortOrder?.ToLower() == "asc" 
                        ? result.Agencies.OrderBy(a => a.Properties)
                        : result.Agencies.OrderByDescending(a => a.Properties),
                    "customers" => sortOrder?.ToLower() == "asc"
                        ? result.Agencies.OrderBy(a => a.Customers)
                        : result.Agencies.OrderByDescending(a => a.Customers),
                    "requests" => sortOrder?.ToLower() == "asc"
                        ? result.Agencies.OrderBy(a => a.Requests)
                        : result.Agencies.OrderByDescending(a => a.Requests),
                    "soldproperties" => sortOrder?.ToLower() == "asc"
                        ? result.Agencies.OrderBy(a => a.SoldProperties)
                        : result.Agencies.OrderByDescending(a => a.SoldProperties),
                    "appointments" => sortOrder?.ToLower() == "asc"
                        ? result.Agencies.OrderBy(a => a.Appointments)
                        : result.Agencies.OrderByDescending(a => a.Appointments),
                    "commissions" => sortOrder?.ToLower() == "asc"
                        ? result.Agencies.OrderBy(a => a.Commissions)
                        : result.Agencies.OrderByDescending(a => a.Commissions),
                    _ => result.Agencies.OrderByDescending(a => a.SoldProperties) // Default: ordina per vendite decrescenti
                };

                // Limita a Top 5 e converte in lista
                result.Agencies = sortedAgencies.Take(5).ToList();

                // Salva in cache per 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Top Agenzie calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Top Agenzie: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Top Agenzie: {ex.Message}");
            }
        }

        public async Task<TopAgentsDataModel> GetTopAgentsData(string? userId, int? year, string? sortBy = null, string? sortOrder = "desc")
        {
            try
            {
                // Genera chiave cache basata su userId, year, sortBy e sortOrder
                var cacheKey = $"TopAgentsData_{userId}_{year ?? DateTime.UtcNow.Year}_{sortBy ?? "SoldProperties"}_{sortOrder ?? "desc"}";

                // Verifica se i dati sono in cache
                if (_cache.TryGetValue(cacheKey, out TopAgentsDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Top Agenti recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new TopAgentsDataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"UserId non specificato per GetTopAgentsData");
                    return result;
                }

                // Determina l'adminId in base al ruolo dell'utente
                ApplicationUser? currentUser = await _userManager.FindByIdAsync(userId);
                string? adminId = null;

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    
                    if (roles.Contains("Admin"))
                    {
                        adminId = currentUser.Id;
                    }
                    else if (roles.Contains("Agency"))
                    {
                        adminId = currentUser.AdminId ?? currentUser.Id;
                    }
                    else if (roles.Contains("Agent"))
                    {
                        adminId = currentUser.AdminId;
                    }
                }

                // Ottieni tutti gli userId nella cerchia per filtrare immobili/appuntamenti/richieste
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Recupera tutti gli agenti nella cerchia dell'utente
                // Usa circleUserIds per filtrare solo gli agenti nella cerchia
                var allAgents = await _userManager.GetUsersInRoleAsync("Agent");
                IQueryable<ApplicationUser> agentsQuery = _unitOfWork.dbContext.Users
                    .Where(u => allAgents.Select(a => a.Id).Contains(u.Id) && circleUserIds.Contains(u.Id));

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Admin: mostra tutti gli agenti della sua cerchia (già filtrati da circleUserIds)
                        // circleUserIds include già tutti gli agenti dell'admin (diretti e delle sue agenzie)
                    }
                    else if (roles.Contains("Agency"))
                    {
                        // Agency: mostra solo i suoi agenti
                        agentsQuery = agentsQuery.Where(u => u.AdminId == currentUser.Id);
                    }
                    else if (roles.Contains("Agent"))
                    {
                        // Agent: mostra solo se stesso
                        agentsQuery = agentsQuery.Where(u => u.Id == currentUser.Id);
                    }
                }

                var agentsList = await agentsQuery.ToListAsync();
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Per ogni agente, calcola i dati aggregati
                foreach (var agent in agentsList)
                {
                    var agentId = agent.Id;
                    
                    // Recupera immobili dell'agente
                    var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   p.UserId == agentId);
                    
                    var allAgentProperties = await propertiesQuery.ToListAsync();

                    // SoldProperties: immobili venduti nell'anno
                    var soldProperties = allAgentProperties
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year == currentYear;
                        })
                        .Count();

                    // LoadedProperties: immobili caricati nell'anno
                    var loadedProperties = allAgentProperties
                        .Where(p => p.CreationDate.Year == currentYear)
                        .Count();

                    // Requests: richieste dell'agente create nell'anno
                    var requests = await _unitOfWork.dbContext.Requests
                        .Where(r => r.UserId != null && r.UserId == agentId)
                        .Where(r => r.CreationDate.Year == currentYear)
                        .CountAsync();

                    // Appointments: appuntamenti dell'agente nell'anno
                    var appointments = await _unitOfWork.dbContext.Calendars
                        .Where(c => c.UserId == agentId)
                        .Where(c => c.EventStartDate.Year == currentYear)
                        .Where(c => !c.Cancelled)
                        .CountAsync();

                    // Commissions: guadagni totali (somma EffectiveCommission degli immobili venduti)
                    var commissions = allAgentProperties
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year == currentYear;
                        })
                        .Sum(p => (decimal)p.EffectiveCommission);

                    // Costruisci location (City, Province)
                    var location = string.IsNullOrEmpty(agent.City) 
                        ? string.Empty 
                        : $"{agent.City}" + (string.IsNullOrEmpty(agent.Province) ? string.Empty : $", {agent.Province}");

                    result.Agents.Add(new TopAgentItemModel
                    {
                        Id = agentId,
                        Name = agent.UserName ?? $"{agent.FirstName} {agent.LastName}".Trim() ?? "Agente",
                        Location = location,
                        SoldProperties = soldProperties,
                        LoadedProperties = loadedProperties,
                        Requests = requests,
                        Appointments = appointments,
                        Commissions = commissions
                    });
                }

                // Ordina dinamicamente in base al parametro sortBy
                var sortedAgents = sortBy?.ToLower() switch
                {
                    "soldproperties" => sortOrder?.ToLower() == "asc" 
                        ? result.Agents.OrderBy(a => a.SoldProperties)
                        : result.Agents.OrderByDescending(a => a.SoldProperties),
                    "loadedproperties" => sortOrder?.ToLower() == "asc"
                        ? result.Agents.OrderBy(a => a.LoadedProperties)
                        : result.Agents.OrderByDescending(a => a.LoadedProperties),
                    "requests" => sortOrder?.ToLower() == "asc"
                        ? result.Agents.OrderBy(a => a.Requests)
                        : result.Agents.OrderByDescending(a => a.Requests),
                    "appointments" => sortOrder?.ToLower() == "asc"
                        ? result.Agents.OrderBy(a => a.Appointments)
                        : result.Agents.OrderByDescending(a => a.Appointments),
                    "commissions" => sortOrder?.ToLower() == "asc"
                        ? result.Agents.OrderBy(a => a.Commissions)
                        : result.Agents.OrderByDescending(a => a.Commissions),
                    _ => result.Agents.OrderByDescending(a => a.SoldProperties) // Default: ordina per vendite decrescenti
                };

                // Limita a Top 5 e converte in lista
                result.Agents = sortedAgents.Take(5).ToList();

                // Salva in cache per 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Top Agenti calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Top Agenti: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Top Agenti: {ex.Message}");
            }
        }

        public async Task<TopZonesDataModel> GetTopZonesData(string? userId)
        {
            try
            {
                // Genera chiave cache basata su userId
                var cacheKey = $"TopZonesData_{userId}";

                // Verifica se i dati sono in cache
                if (_cache.TryGetValue(cacheKey, out TopZonesDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Top Zone recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new TopZonesDataModel();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"UserId non specificato per GetTopZonesData");
                    return result;
                }

                // Ottieni tutti gli userId nella cerchia per filtrare immobili/richieste
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                var now = DateTime.UtcNow;

                // ========== TOP ZONE IMMOBILI ==========
                // Recupera immobili non venduti con incarico valido nella cerchia
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                               circleUserIds.Contains(p.UserId) &&
                               !p.Sold && // Solo immobili non venduti
                               (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd >= now)); // Incarico valido

                var allProperties = await propertiesQuery.ToListAsync();

                // Raggruppa per City
                var propertiesByCity = allProperties
                    .Where(p => !string.IsNullOrEmpty(p.City))
                    .GroupBy(p => p.City)
                    .Select(g => new
                    {
                        City = g.Key!,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                var totalProperties = allProperties.Count;

                foreach (var item in propertiesByCity)
                {
                    result.PropertiesZones.Add(new TopZoneItemModel
                    {
                        Name = item.City,
                        Count = item.Count,
                        Percentage = totalProperties > 0 ? (int)Math.Round((double)item.Count / totalProperties * 100) : 0
                    });
                }

                // ========== TOP ZONE RICHIESTE ==========
                // Recupera tutte le richieste nella cerchia (filtra per UserId nella cerchia)
                var requestsQuery = _unitOfWork.dbContext.Requests
                    .Where(r => r.UserId != null && circleUserIds.Contains(r.UserId));

                var allRequests = await requestsQuery.ToListAsync();

                // Estrai tutte le città citate (il campo City può contenere più città separate da virgola)
                // Esempio: "Olevano Romano, Palestrina, Palombara Sabina" -> ["Olevano Romano", "Palestrina", "Palombara Sabina"]
                var allCitiesList = new List<string>();
                foreach (var request in allRequests)
                {
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        // Separa le città per virgola e pulisci gli spazi
                        var cities = request.City
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Where(c => !string.IsNullOrWhiteSpace(c));
                        
                        allCitiesList.AddRange(cities);
                    }
                }

                // Conta quante volte ogni città viene citata
                // Esempio: Roma citata in 3 richieste, Cave in 2, Olevano in 1
                var allCitiesCount = allCitiesList
                    .GroupBy(city => city)
                    .Select(g => new
                    {
                        City = g.Key,
                        Count = g.Count() // Numero di volte che questa città viene citata
                    })
                    .OrderByDescending(x => x.Count) // Ordina per numero di citazioni della città
                    .ToList();

                // Calcola il totale delle citazioni di città (per calcolare le percentuali)
                var totalCityCitations = allCitiesCount.Sum(x => x.Count);

                // Prendi solo le Top 5 città più citate
                var requestsByCity = allCitiesCount.Take(5).ToList();

                foreach (var item in requestsByCity)
                {
                    result.RequestsZones.Add(new TopZoneItemModel
                    {
                        Name = item.City,
                        Count = item.Count, // Numero di volte che questa città viene citata
                        Percentage = totalCityCitations > 0 ? (int)Math.Round((double)item.Count / totalCityCitations * 100) : 0
                    });
                }

                // Salva in cache per 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Top Zone calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Top Zone: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Top Zone: {ex.Message}");
            }
        }

        public async Task<TopTypologiesDataModel> GetTopTypologiesData(string? userId)
        {
            try
            {
                // Genera chiave cache basata su userId
                var cacheKey = $"TopTypologiesData_{userId}";

                // Verifica cache
                if (_cache.TryGetValue(cacheKey, out TopTypologiesDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Top Tipologie recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new TopTypologiesDataModel();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId non specificato per GetTopTypologiesData");
                    return result;
                }

                // Cerchia
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                var now = DateTime.UtcNow;

                // ========== TOP CATEGORIE PORTAFOGLIO (Tipologie immobili) ==========
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                !p.Sold &&
                                (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd >= now));

                var allProperties = await propertiesQuery.ToListAsync();

                // Estrai tipologie, supportando valori multipli separati da virgola
                var allPropertyTypologies = new List<string>();
                foreach (var property in allProperties)
                {
                    if (!string.IsNullOrEmpty(property.Typology))
                    {
                        var typologies = property.Typology
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t));
                        allPropertyTypologies.AddRange(typologies);
                    }
                }

                var propertyTypologyCounts = allPropertyTypologies
                    .GroupBy(t => t)
                    .Select(g => new
                    {
                        Typology = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var totalPropertyTypologies = propertyTypologyCounts.Sum(x => x.Count);
                var topPropertyTypologies = propertyTypologyCounts.Take(5).ToList();

                foreach (var item in topPropertyTypologies)
                {
                    result.CategoriesPortfolio.Add(new TopTypologyItemModel
                    {
                        Name = item.Typology,
                        Count = item.Count,
                        Percentage = totalPropertyTypologies > 0 ? (int)Math.Round((double)item.Count / totalPropertyTypologies * 100) : 0
                    });
                }

                // ========== TOP CATEGORIE RICHIESTE (Tipologie richieste) ==========
                var requestsQuery = _unitOfWork.dbContext.Requests
                    .Where(r => r.UserId != null && circleUserIds.Contains(r.UserId));

                var allRequests = await requestsQuery.ToListAsync();

                var allRequestTypologies = new List<string>();
                foreach (var request in allRequests)
                {
                    if (!string.IsNullOrEmpty(request.PropertyType))
                    {
                        var typologies = request.PropertyType
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t));
                        allRequestTypologies.AddRange(typologies);
                    }
                }

                var requestTypologyCounts = allRequestTypologies
                    .GroupBy(t => t)
                    .Select(g => new
                    {
                        Typology = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var totalRequestTypologies = requestTypologyCounts.Sum(x => x.Count);
                var topRequestTypologies = requestTypologyCounts.Take(5).ToList();

                foreach (var item in topRequestTypologies)
                {
                    result.CategoriesRequests.Add(new TopTypologyItemModel
                    {
                        Name = item.Typology,
                        Count = item.Count,
                        Percentage = totalRequestTypologies > 0 ? (int)Math.Round((double)item.Count / totalRequestTypologies * 100) : 0
                    });
                }

                // Cache 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Top Tipologie calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Top Tipologie: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Top Tipologie: {ex.Message}");
            }
        }

        public async Task<TopEarningsDataModel> GetTopEarningsData(string? userId, int? year)
        {
            try
            {
                var cacheKey = $"TopEarningsData_{userId}_{year ?? DateTime.UtcNow.Year}";

                if (_cache.TryGetValue(cacheKey, out TopEarningsDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Top Guadagni recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new TopEarningsDataModel();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId non specificato per GetTopEarningsData");
                    return result;
                }

                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                var now = DateTime.UtcNow;
                var targetYear = year ?? now.Year;

                // ===== Portafoglio (non venduti, incarico valido) =====
                var portfolioPropsQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                !p.Sold &&
                                (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd >= now));

                var portfolioProps = await portfolioPropsQuery.ToListAsync();

                var portfolioItems = portfolioProps
                    .Select(p => new TopEarningItemModel
                    {
                        Id = p.Id.ToString(),
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        UserFirstName = p.User?.FirstName ?? string.Empty,
                        Price = p.Price,
                        EffectiveCommission = p.EffectiveCommission
                    })
                    .OrderByDescending(x => x.EffectiveCommission)
                    .Take(5)
                    .ToList();

                result.Portfolio = portfolioItems;
                result.TotalPortfolioCommission = (decimal)portfolioProps.Sum(p => p.EffectiveCommission);

                // ===== Vendite anno corrente (Sold=true, anno = filtro) =====
                var salesPropsQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                p.Sold);

                var salesProps = await salesPropsQuery.ToListAsync();

                var salesYearProps = salesProps.Where(p =>
                {
                    var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                        ? p.UpdateDate
                        : p.CreationDate;
                    return soldDate.Year == targetYear;
                }).ToList();

                var salesItems = salesYearProps
                    .Select(p => new TopEarningItemModel
                    {
                        Id = p.Id.ToString(),
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        UserFirstName = p.User?.FirstName ?? string.Empty,
                        Price = p.Price,
                        EffectiveCommission = p.EffectiveCommission
                    })
                    .OrderByDescending(x => x.EffectiveCommission)
                    .Take(5)
                    .ToList();

                result.SalesYear = salesItems;
                result.TotalSalesYearCommission = (decimal)salesYearProps.Sum(p => p.EffectiveCommission);

                // Cache 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Top Guadagni calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Top Guadagni: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Top Guadagni: {ex.Message}");
            }
        }

        public async Task<AnalyticsDataModel> GetAnalyticsData(string? userId, int year, string? agencyId = null)
        {
            try
            {
                // Genera chiave cache basata su userId, year e agencyId
                var cacheKey = $"AnalyticsData_{userId}_{year}_{agencyId ?? "all"}";

                // Verifica cache
                if (_cache.TryGetValue(cacheKey, out AnalyticsDataModel? cachedData))
                {
                    _logger.LogInformation($"Dati Analytics recuperati dalla cache per chiave: {cacheKey}");
                    return cachedData!;
                }

                var result = new AnalyticsDataModel();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId non specificato per GetAnalyticsData");
                    return result;
                }

                // Cerchia admin
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Filtro agenzia (se presente)
                string? filterAgencyId = null;
                if (!string.IsNullOrEmpty(agencyId))
                {
                    if (agencyId.StartsWith("agency_"))
                    {
                        filterAgencyId = agencyId.Replace("agency_", "");
                    }
                    else if (agencyId.StartsWith("agent_"))
                    {
                        filterAgencyId = agencyId.Replace("agent_", "");
                    }
                    else
                    {
                        filterAgencyId = agencyId;
                    }
                }

                // Helper: formatta mese nel formato italiano (es: "gen 24")
                string FormatMonthKey(int month, int year)
                {
                    var date = new DateTime(year, month, 1);
                    return date.ToString("MMM yy", new System.Globalization.CultureInfo("it-IT"));
                }

                // Inizializza tutti i mesi dell'anno
                var allMonths = new Dictionary<string, int>();
                for (int month = 1; month <= 12; month++)
                {
                    allMonths[FormatMonthKey(month, year)] = 0;
                }

                // ========== RICHIESTE ==========
                var requestsQuery = _unitOfWork.dbContext.Requests
                    .Where(r => r.UserId != null && circleUserIds.Contains(r.UserId));

                // Applica filtro agenzia se presente
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    requestsQuery = requestsQuery.Where(r => r.UserId == filterAgencyId);
                }

                var allRequests = await requestsQuery.ToListAsync();

                // Richieste inserite (per CreationDate)
                var requestsInserted = allRequests
                    .Where(r => r.CreationDate.Year == year)
                    .GroupBy(r => FormatMonthKey(r.CreationDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Richieste evase (per UpdateDate quando Closed == true)
                var requestsClosed = allRequests
                    .Where(r => r.Closed && r.UpdateDate.Year == year)
                    .GroupBy(r => FormatMonthKey(r.UpdateDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                result.Requests.MonthlyData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in requestsInserted)
                {
                    result.Requests.MonthlyData[kvp.Key] = kvp.Value;
                }
                result.Requests.Total = requestsInserted.Values.Sum();

                result.Requests.ClosedData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in requestsClosed)
                {
                    result.Requests.ClosedData[kvp.Key] = kvp.Value;
                }

                // ========== IMMOBILI ==========
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) && circleUserIds.Contains(p.UserId));

                // Applica filtro agenzia se presente
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    propertiesQuery = propertiesQuery.Where(p => p.UserId == filterAgencyId);
                }

                var allProperties = await propertiesQuery.ToListAsync();

                // Immobili inseriti (Sold == false, per CreationDate)
                var propertiesInserted = allProperties
                    .Where(p => !p.Sold && p.CreationDate.Year == year)
                    .GroupBy(p => FormatMonthKey(p.CreationDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Immobili venduti (Sold == true, per UpdateDate o CreationDate se UpdateDate non valido)
                var propertiesSold = allProperties
                    .Where(p => p.Sold)
                    .Select(p => new
                    {
                        p.Id,
                        SoldDate = p.UpdateDate != default(DateTime) && p.UpdateDate.Year == year
                            ? p.UpdateDate
                            : (p.CreationDate.Year == year ? p.CreationDate : default(DateTime))
                    })
                    .Where(p => p.SoldDate != default(DateTime) && p.SoldDate.Year == year)
                    .GroupBy(p => FormatMonthKey(p.SoldDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                result.Properties.MonthlyData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in propertiesInserted)
                {
                    result.Properties.MonthlyData[kvp.Key] = kvp.Value;
                }
                result.Properties.Total = propertiesInserted.Values.Sum();

                result.Properties.SoldData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in propertiesSold)
                {
                    result.Properties.SoldData[kvp.Key] = kvp.Value;
                }

                // ========== CLIENTI ==========
                var customersQuery = _unitOfWork.dbContext.Customers
                    .Where(c => !string.IsNullOrEmpty(c.UserId) && circleUserIds.Contains(c.UserId));

                // Applica filtro agenzia se presente
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    customersQuery = customersQuery.Where(c => c.UserId == filterAgencyId);
                }

                var allCustomers = await customersQuery.ToListAsync();

                // Clienti venditori (Seller == true, per CreationDate)
                var customersSellers = allCustomers
                    .Where(c => c.Seller && c.CreationDate.Year == year)
                    .GroupBy(c => FormatMonthKey(c.CreationDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Clienti acquirenti (Buyer == true, per CreationDate)
                var customersBuyers = allCustomers
                    .Where(c => c.Buyer && c.CreationDate.Year == year)
                    .GroupBy(c => FormatMonthKey(c.CreationDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                result.Customers.MonthlyData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in customersSellers)
                {
                    result.Customers.MonthlyData[kvp.Key] = kvp.Value;
                }
                result.Customers.Total = customersSellers.Values.Sum() + customersBuyers.Values.Sum();

                result.Customers.BuyersData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in customersBuyers)
                {
                    result.Customers.BuyersData[kvp.Key] = kvp.Value;
                }

                // ========== APPUNTAMENTI ==========
                var appointmentsQuery = _unitOfWork.dbContext.Calendars
                    .Where(a => !string.IsNullOrEmpty(a.UserId) && circleUserIds.Contains(a.UserId));

                // Applica filtro agenzia se presente
                if (!string.IsNullOrEmpty(filterAgencyId))
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.UserId == filterAgencyId);
                }

                var allAppointments = await appointmentsQuery.ToListAsync();

                // Appuntamenti fissati (per CreationDate)
                var appointmentsFixed = allAppointments
                    .Where(a => a.CreationDate.Year == year)
                    .GroupBy(a => FormatMonthKey(a.CreationDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Appuntamenti confermati (per UpdateDate quando Confirmed == true)
                var appointmentsConfirmed = allAppointments
                    .Where(a => a.Confirmed && a.UpdateDate.Year == year)
                    .GroupBy(a => FormatMonthKey(a.UpdateDate.Month, year))
                    .ToDictionary(g => g.Key, g => g.Count());

                result.Appointments.MonthlyData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in appointmentsFixed)
                {
                    result.Appointments.MonthlyData[kvp.Key] = kvp.Value;
                }
                result.Appointments.Total = appointmentsFixed.Values.Sum();

                result.Appointments.ConfirmedData = new Dictionary<string, int>(allMonths);
                foreach (var kvp in appointmentsConfirmed)
                {
                    result.Appointments.ConfirmedData[kvp.Key] = kvp.Value;
                }

                // Cache 5 minuti
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation($"Dati Analytics calcolati e salvati in cache per chiave: {cacheKey}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati Analytics: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati Analytics: {ex.Message}");
            }
        }
    }
}

