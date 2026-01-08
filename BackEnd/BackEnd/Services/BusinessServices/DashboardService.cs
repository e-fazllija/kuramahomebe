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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId, agencyId e year
                // Usa "current" per modalità "Corrente" (year null) per distinguerla dall'anno specifico
                // var cacheKey = $"MapData_{userId}_{agencyId ?? "all"}_{(year.HasValue ? year.Value.ToString() : "current")}";

                // Verifica se i dati sono in cache
                // if (_cache.TryGetValue(cacheKey, out MapDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati mappa recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

                var result = new MapDataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;
                bool isCurrentMode = !year.HasValue; // "Corrente" quando year è null

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

                // Applica filtro per anno se specificato (filtra per data creazione agenzia)
                // Se year è null (modalità "Corrente"), mostra tutte le agenzie attive (nessun filtro)
                if (year.HasValue)
                {
                    // Filtra solo le agenzie che esistevano nell'anno specificato
                    // Mostra agenzie create prima o durante l'anno selezionato
                    var targetYear = year.Value;
                    agenciesBaseQuery = agenciesBaseQuery.Where(u => u.CreationDate.Year <= targetYear);
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

                // Aggiungi l'admin stesso se è Admin e non è già nella lista
                // Includi l'admin anche se il filtro è attivo per la sua agenzia (per modalità "Corrente")
                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);
                    if (roles.Contains("Admin"))
                    {
                        // Verifica se il filtro è per l'admin stesso
                        bool isFilterForAdminAgency = !string.IsNullOrEmpty(filterAgencyId) && 
                            (filterAgencyId == currentUser.Id || 
                             (filterAgencyId.StartsWith("agency_") && filterAgencyId.Replace("agency_", "") == currentUser.Id));
                        
                        // Aggiungi l'admin se: modalità "Corrente" OPPURE il filtro è per l'admin stesso
                        if ((isCurrentMode || isFilterForAdminAgency) && !result.Agencies.Any(a => a.Id == currentUser.Id))
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


                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Salva in cache per 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati mappa calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId, agencyId e year
                // Usa "current" per modalità "Corrente" (year null) per distinguerla dall'anno specifico
                // var cacheKey = $"Widget3Data_{userId}_{agencyId ?? "all"}_{(year.HasValue ? year.Value.ToString() : "current")}";

                // Verifica se i dati sono in cache
                // if (_cache.TryGetValue(cacheKey, out Widget3DataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Widget3 recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData;
                // }

                var result = new Widget3DataModel();
                var now = DateTime.UtcNow;
                var currentYear = year ?? now.Year;
                bool isCurrentMode = !year.HasValue; // "Corrente" quando year è null

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
                // Esclude immobili cancellati/archiviati
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => circleUserIds.Contains(p.UserId) && !p.Archived); // Esclude immobili archiviati/cancellati

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

                // Seleziona tutti gli immobili (con filtri applicati)
                // Per "Corrente": tutti gli immobili degli ultimi 6 anni (non cancellati)
                // Per anno specifico: immobili di quell'anno
                IQueryable<RealEstateProperty> filteredPropertiesQuery = propertiesQuery;

                if (isCurrentMode)
                {
                    // Modalità "Corrente": ultimi 6 anni
                    // Include TUTTI gli immobili degli ultimi 6 anni (inseriti o venduti)
                    // Per inseriti: CreationDate negli ultimi 6 anni
                    // Per venduti: soldDate (UpdateDate o CreationDate) negli ultimi 6 anni
                    var startYear = currentYear - 5; // Ultimi 6 anni incluso l'anno corrente
                    filteredPropertiesQuery = propertiesQuery
                        .Where(p => 
                            // Immobili inseriti negli ultimi 6 anni
                            (p.CreationDate.Year >= startYear && p.CreationDate.Year <= currentYear) ||
                            // Immobili venduti negli ultimi 6 anni (usa UpdateDate se valida, altrimenti CreationDate)
                            (p.Sold && (
                                (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1) && 
                                 p.UpdateDate.Year >= startYear && p.UpdateDate.Year <= currentYear) ||
                                ((p.UpdateDate == default(DateTime) || p.UpdateDate == new DateTime(1, 1, 1)) && 
                                 p.CreationDate.Year >= startYear && p.CreationDate.Year <= currentYear)
                            )));
                }
                else
                {
                    // Modalità anno specifico: solo quell'anno
                    // Include SOLO:
                    // 1. Immobili inseriti nel currentYear (CreationDate.Year == currentYear)
                    // 2. Immobili venduti nel currentYear (UpdateDate.Year == currentYear quando Sold == true)
                    filteredPropertiesQuery = propertiesQuery
                        .Where(p => p.CreationDate.Year == currentYear || // Inseriti nel currentYear
                                   (p.Sold && p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1) && p.UpdateDate.Year == currentYear)); // Venduti nel currentYear (con UpdateDate valida)
                }

                var allProperties = await filteredPropertiesQuery.ToListAsync();

                // Genera array dei periodi (mesi o anni) in base alla modalità
                var periods = new List<string>();
                
                if (isCurrentMode)
                {
                    // Modalità "Corrente": genera chiavi per anni (ultimi 6 anni)
                    // Formato: "2021 I", "2021 V", "2022 I", "2022 V", ... "2026 I", "2026 V"
                    var startYear = currentYear - 5; // Ultimi 6 anni
                    for (int y = startYear; y <= currentYear; y++)
                    {
                        periods.Add($"{y} I"); // Inseriti
                        periods.Add($"{y} V"); // Venduti
                    }
                    result.Months = periods; // Usa Months anche per anni per compatibilità
                    _logger.LogInformation($"Anni generati per modalità Corrente: {string.Join(", ", periods)}");
                }
                else
                {
                    // Modalità anno specifico: genera mesi
                    // Usa formato compatibile con frontend: "gen 25" (senza punto)
                    var culture = new System.Globalization.CultureInfo("it-IT");
                    for (int i = 0; i < 12; i++)
                    {
                        var date = new DateTime(currentYear, i + 1, 1);
                        // Formato: "gen 25" (rimuove eventuali punti dal formato standard)
                        var monthKey = date.ToString("MMM yy", culture).Replace(".", "").Trim();
                        periods.Add(monthKey);
                    }
                    result.Months = periods;
                    _logger.LogInformation($"Mesi generati per anno {currentYear}: {string.Join(", ", periods)}");
                }

                // Inizializza i dizionari per ogni periodo (mese o anno)
                var propertiesData = new PropertiesDataModel();
                var soldPropertiesData = new PropertiesDataModel();
                var commissionsMonthly = new Dictionary<string, decimal>();

                foreach (var period in periods)
                {
                    propertiesData.Sale[period] = 0;
                    propertiesData.Rent[period] = 0;
                    propertiesData.Auction[period] = 0;
                    soldPropertiesData.Sale[period] = 0;
                    soldPropertiesData.Rent[period] = 0;
                    soldPropertiesData.Auction[period] = 0;
                    commissionsMonthly[period] = 0;
                }

                // Processa ogni immobile
                foreach (var property in allProperties)
                {
                    if (isCurrentMode)
                    {
                        // Modalità "Corrente": aggregazione per anno
                        var creationYear = property.CreationDate.Year;
                        var insertedKey = $"{creationYear} I";
                        
                        // Verifica che l'anno sia negli ultimi 6 anni
                        var startYear = currentYear - 5;
                        if (creationYear >= startYear && creationYear <= currentYear && periods.Contains(insertedKey))
                        {
                            // Per "Corrente", mostra tutti gli inseriti (non cancellati) indipendentemente da scadenza
                            // Ma NON mostrare se l'immobile è stato venduto (il venduto va nel suo anno)
                            if (!property.Sold)
                            {
                                // Immobile inserito ma non venduto: mostra nell'anno di inserimento
                                if (property.Auction)
                                {
                                    propertiesData.Auction[insertedKey]++;
                                }
                                else if (property.Status == "Vendita")
                                {
                                    propertiesData.Sale[insertedKey]++;
                                }
                                else if (property.Status == "Affitto")
                                {
                                    propertiesData.Rent[insertedKey]++;
                                }
                            }
                        }

                        // Gestione immobili venduti
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

                            var soldYear = soldDate.Year;
                            var soldKey = $"{soldYear} V";
                            
                            if (soldYear >= startYear && soldYear <= currentYear && periods.Contains(soldKey))
                            {
                                // Immobile venduto: mostra nell'anno di vendita
                                if (property.Auction)
                                {
                                    soldPropertiesData.Auction[soldKey]++;
                                }
                                else if (property.Status == "Vendita")
                                {
                                    soldPropertiesData.Sale[soldKey]++;
                                }
                                else if (property.Status == "Affitto")
                                {
                                    soldPropertiesData.Rent[soldKey]++;
                                }

                                // Provvigioni incassate nell'anno di vendita
                                var commission = (decimal)property.EffectiveCommission;
                                if (commission > 0)
                                {
                                    commissionsMonthly[soldKey] += commission;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Modalità anno specifico: aggregazione per mese
                        // Per anno specifico, mostra TUTTI gli immobili inseriti/venduti in quell'anno
                        // indipendentemente dallo stato attuale dell'incarico (come richiesto: anche se scaduti/venduti dopo)
                        // Data per immobili inseriti (usa CreationDate)
                        var creationDate = property.CreationDate;
                        var creationMonth = creationDate.ToString("MMM yy", new System.Globalization.CultureInfo("it-IT")).Replace(".", "").Trim();

                        if (creationDate.Year == currentYear && periods.Contains(creationMonth))
                        {
                            // Per anno specifico, mostra tutti gli inseriti di quell'anno
                            // NON verifichiamo se l'incarico è ancora valido oggi (come richiesto)
                            // Solo verifichiamo che non sia stato venduto (il venduto va nel mese di vendita)
                            if (!property.Sold)
                            {
                                // Immobile inserito ma non venduto: conta nell'anno di inserimento
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
                            // Se è stato venduto, verrà contato nel mese di vendita (vedi logica sotto)
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

                            if (soldDate.Year == currentYear && periods.Contains(soldMonth))
                            {
                                // Per anno specifico, mostra TUTTI i venduti di quell'anno
                                // Verifica solo che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                                bool isAssignmentValid = false;
                                if (property.AssignmentEnd == default(DateTime) || property.AssignmentEnd == new DateTime(1, 1, 1))
                                {
                                    // Incarico senza scadenza, sempre valido
                                    isAssignmentValid = true;
                                }
                                else
                                {
                                    // L'incarico deve essere valido al momento della vendita (non oggi)
                                    isAssignmentValid = property.AssignmentEnd > soldDate;
                                }

                                // Categorizza per Status e Auction (solo se l'incarico era valido al momento della vendita)
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

                                    // Aggiungi provvigione al mese di vendita
                                    var commission = (decimal)property.EffectiveCommission;
                                    if (commission > 0)
                                    {
                                        _logger.LogInformation($"Immobile {property.Id}: EffectiveCommission={commission}, soldMonth={soldMonth}, soldDate={soldDate:yyyy-MM-dd}, AssignmentEnd={property.AssignmentEnd:yyyy-MM-dd}");
                                    }
                                    commissionsMonthly[soldMonth] += commission;
                                }
                                else
                                {
                                    // Immobile venduto ma con incarico già scaduto al momento della vendita, non includere
                                    _logger.LogWarning($"Immobile {property.Id} venduto ma incarico già scaduto al momento della vendita: soldDate={soldDate:yyyy-MM-dd}, AssignmentEnd={property.AssignmentEnd:yyyy-MM-dd}");
                                }
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
                if (isCurrentMode)
                {
                    // Modalità "Corrente": somma di tutti gli ultimi 6 anni
                    var startYear = currentYear - 5;
                    
                    // Portafoglio = somma EffectiveCommission degli immobili non venduti degli ultimi 6 anni
                    // che sono ancora attivi (incarico non scaduto)
                    result.TotalCommissionsPortfolio = allProperties
                        .Where(p => !p.Sold) // Solo immobili non venduti
                        .Where(p => p.CreationDate.Year >= startYear && p.CreationDate.Year <= currentYear) // Creati negli ultimi 6 anni
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

                    // Incassati = somma EffectiveCommission degli immobili venduti negli ultimi 6 anni
                    result.TotalCommissionsEarned = allProperties
                        .Where(p => p.Sold) // Solo immobili venduti
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year >= startYear && soldDate.Year <= currentYear; // Venduti negli ultimi 6 anni
                        })
                        .Sum(p => (decimal)p.EffectiveCommission);
                }
                else
                {
                    // Modalità anno specifico: mostra TUTTI gli immobili inseriti/venduti di quell'anno
                    // anche se l'incarico è scaduto dopo (come richiesto per un resoconto affidabile)
                    // Portafoglio = somma EffectiveCommission degli immobili NON venduti inseriti nell'anno
                    // (non verifichiamo se l'incarico è scaduto oggi, mostriamo comunque)
                    result.TotalCommissionsPortfolio = allProperties
                        .Where(p => !p.Sold) // Solo immobili non venduti
                        .Where(p => p.CreationDate.Year == currentYear) // Creati nell'anno selezionato
                        .Sum(p => (decimal)p.EffectiveCommission);

                    // Incassati = somma EffectiveCommission degli immobili venduti nell'anno
                    // ma solo se l'incarico era valido AL MOMENTO DELLA VENDITA (non oggi)
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
                            // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                            {
                                return true; // Incarico senza scadenza, sempre valido
                            }
                            // L'incarico deve essere valido al momento della vendita (non oggi)
                            return p.AssignmentEnd > soldDate;
                        })
                        .Sum(p => (decimal)p.EffectiveCommission);
                }

                // Calcola totali valori immobili (Portafoglio e Venduti)
                if (isCurrentMode)
                {
                    // Modalità "Corrente": ultimi 6 anni
                    var startYear = currentYear - 5;
                    
                    // Portafoglio = somma Price degli immobili non venduti degli ultimi 6 anni
                    // che sono ancora attivi (incarico non scaduto)
                    var portfolioPropertiesForValue = allProperties
                        .Where(p => !p.Sold) // Solo immobili non venduti
                        .Where(p => p.CreationDate.Year >= startYear && p.CreationDate.Year <= currentYear) // Creati negli ultimi 6 anni
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
                        .ToList();
                    
                    result.TotalPortfolioValue = (decimal)portfolioPropertiesForValue.Sum(p => p.Price);

                    // Venduti = somma Price degli immobili venduti negli ultimi 6 anni
                    var soldPropertiesForValue = allProperties
                        .Where(p => p.Sold) // Solo immobili venduti
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year >= startYear && soldDate.Year <= currentYear; // Venduti negli ultimi 6 anni
                        })
                        .ToList();
                    
                    result.TotalSoldValue = (decimal)soldPropertiesForValue.Sum(p => p.Price);
                }
                else
                {
                    // Modalità anno specifico
                    // Portafoglio = somma Price degli immobili NON venduti inseriti nell'anno con incarico valido
                    var portfolioPropertiesForValue = allProperties
                        .Where(p => !p.Sold) // Solo immobili non venduti
                        .Where(p => p.CreationDate.Year == currentYear) // Creati nell'anno selezionato
                        .Where(p => 
                        {
                            // Verifica che l'incarico sia valido
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                            {
                                return true; // Incarico senza scadenza, sempre valido
                            }
                            // Altrimenti verifica che non sia scaduto
                            return p.AssignmentEnd > now;
                        })
                        .ToList();
                    
                    result.TotalPortfolioValue = (decimal)portfolioPropertiesForValue.Sum(p => p.Price);

                    // Venduti = somma Price degli immobili venduti nell'anno
                    var soldPropertiesForValue = allProperties
                        .Where(p => p.Sold) // Solo immobili venduti
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            return soldDate.Year == currentYear; // Venduti nell'anno selezionato
                        })
                        .ToList();
                    
                    result.TotalSoldValue = (decimal)soldPropertiesForValue.Sum(p => p.Price);
                }

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Salva in cache per 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Widget3 calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId, year, sortBy e sortOrder
                // var cacheKey = $"TopAgenciesData_{userId}_{year ?? DateTime.UtcNow.Year}_{sortBy ?? "SoldProperties"}_{sortOrder ?? "desc"}";

                // Verifica se i dati sono in cache
                // if (_cache.TryGetValue(cacheKey, out TopAgenciesDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Top Agenzie recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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

                    var nowDateOnly = DateTime.UtcNow.Date;
                    
                    // Recupera TUTTI gli immobili dell'agenzia (non cancellati)
                    // Per properties, customers, requests, appointments usiamo solo quelli con incarico valido OGGI
                    // Per soldProperties e commissions dobbiamo verificare l'incarico AL MOMENTO DELLA VENDITA, non oggi
                    var propertiesQueryForActive = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   ((p.User != null && !string.IsNullOrEmpty(p.User.AdminId) && p.User.AdminId == agencyId) || p.UserId == agencyId) &&
                                   !p.Archived && // Escludi cancellati
                                   (p.AssignmentEnd == default(DateTime) || 
                                    p.AssignmentEnd == new DateTime(1, 1, 1) || 
                                    p.AssignmentEnd.Date >= nowDateOnly));
                    
                    // Query per TUTTI gli immobili (inclusi quelli con incarico scaduto) per verificare vendite e commissioni
                    var allAgencyPropertiesForSalesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   ((p.User != null && !string.IsNullOrEmpty(p.User.AdminId) && p.User.AdminId == agencyId) || p.UserId == agencyId) &&
                                   !p.Archived); // Solo escludi cancellati, non filtrare per incarico
                    
                    var allAgencyPropertiesActive = await propertiesQueryForActive.ToListAsync();
                    var allAgencyPropertiesAll = await allAgencyPropertiesForSalesQuery.ToListAsync();

                    // Properties: totale immobili dell'agenzia creati nell'anno (solo con incarico valido oggi)
                    var properties = allAgencyPropertiesActive
                        .Where(p => p.CreationDate.Year == currentYear)
                        .Count();

                    // Customers: numero clienti degli agenti dell'agenzia creati nell'anno
                    var customers = await _unitOfWork.dbContext.Customers
                        .Where(c => (c.UserId != null && agencyAgentIds.Contains(c.UserId)) || c.UserId == agencyId)
                        .Where(c => c.CreationDate.Year == currentYear)
                        .CountAsync();

                    // SoldProperties: immobili venduti nell'anno con incarico valido AL MOMENTO DELLA VENDITA
                    var soldProperties = allAgencyPropertiesAll
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            
                            // Deve essere venduto nell'anno selezionato
                            if (soldDate.Year != currentYear)
                                return false;
                            
                            // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                                return true; // Incarico senza scadenza, sempre valido
                            
                            // L'incarico deve essere valido al momento della vendita
                            return p.AssignmentEnd > soldDate;
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
                        .Where(r => !r.Archived) // Escludi richieste cancellate
                        .Where(r => r.CreationDate.Year == currentYear)
                        .CountAsync();

                    // Commissions: guadagni totali (somma EffectiveCommission degli immobili venduti)
                    // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA, non oggi
                    var commissions = allAgencyPropertiesAll
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            
                            // Deve essere venduto nell'anno selezionato
                            if (soldDate.Year != currentYear)
                                return false;
                            
                            // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                                return true; // Incarico senza scadenza, sempre valido
                            
                            // L'incarico deve essere valido al momento della vendita
                            return p.AssignmentEnd > soldDate;
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

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Salva in cache per 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Top Agenzie calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId, year, sortBy e sortOrder
                // var cacheKey = $"TopAgentsData_{userId}_{year ?? DateTime.UtcNow.Year}_{sortBy ?? "SoldProperties"}_{sortOrder ?? "desc"}";

                // Verifica se i dati sono in cache
                // if (_cache.TryGetValue(cacheKey, out TopAgentsDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Top Agenti recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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
                    
                    var nowDateOnly = DateTime.UtcNow.Date;
                    
                    // Recupera immobili dell'agente con incarico valido OGGI (per loadedProperties)
                    var propertiesQueryForActive = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   p.UserId == agentId &&
                                   !p.Archived &&
                                   (p.AssignmentEnd == default(DateTime) || 
                                    p.AssignmentEnd == new DateTime(1, 1, 1) || 
                                    p.AssignmentEnd.Date >= nowDateOnly));
                    
                    // Query per TUTTI gli immobili (inclusi quelli con incarico scaduto) per verificare vendite e commissioni
                    var allAgentPropertiesForSalesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(p => p.User)
                        .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                                   circleUserIds.Contains(p.UserId) && 
                                   p.UserId == agentId &&
                                   !p.Archived); // Solo escludi cancellati, non filtrare per incarico
                    
                    var allAgentPropertiesActive = await propertiesQueryForActive.ToListAsync();
                    var allAgentPropertiesAll = await allAgentPropertiesForSalesQuery.ToListAsync();

                    // SoldProperties: immobili venduti nell'anno con incarico valido AL MOMENTO DELLA VENDITA
                    var soldProperties = allAgentPropertiesAll
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            
                            // Deve essere venduto nell'anno selezionato
                            if (soldDate.Year != currentYear)
                                return false;
                            
                            // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                                return true; // Incarico senza scadenza, sempre valido
                            
                            // L'incarico deve essere valido al momento della vendita
                            return p.AssignmentEnd > soldDate;
                        })
                        .Count();

                    // LoadedProperties: immobili caricati nell'anno (solo con incarico valido oggi)
                    var loadedProperties = allAgentPropertiesActive
                        .Where(p => p.CreationDate.Year == currentYear)
                        .Count();

                    // Requests: richieste dell'agente create nell'anno
                    var requests = await _unitOfWork.dbContext.Requests
                        .Where(r => r.UserId != null && r.UserId == agentId)
                        .Where(r => !r.Archived) // Escludi richieste cancellate
                        .Where(r => r.CreationDate.Year == currentYear)
                        .CountAsync();

                    // Appointments: appuntamenti dell'agente nell'anno
                    var appointments = await _unitOfWork.dbContext.Calendars
                        .Where(c => c.UserId == agentId)
                        .Where(c => c.EventStartDate.Year == currentYear)
                        .Where(c => !c.Cancelled)
                        .CountAsync();

                    // Commissions: guadagni totali (somma EffectiveCommission degli immobili venduti)
                    // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA, non oggi
                    var commissions = allAgentPropertiesAll
                        .Where(p => p.Sold)
                        .Where(p =>
                        {
                            var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                                ? p.UpdateDate
                                : p.CreationDate;
                            
                            // Deve essere venduto nell'anno selezionato
                            if (soldDate.Year != currentYear)
                                return false;
                            
                            // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                            if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                                return true; // Incarico senza scadenza, sempre valido
                            
                            // L'incarico deve essere valido al momento della vendita
                            return p.AssignmentEnd > soldDate;
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

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Salva in cache per 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Top Agenti calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId
                // var cacheKey = $"TopZonesData_{userId}";

                // Verifica se i dati sono in cache
                // if (_cache.TryGetValue(cacheKey, out TopZonesDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Top Zone recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Salva in cache per 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Top Zone calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId
                // var cacheKey = $"TopTypologiesData_{userId}";

                // Verifica cache
                // if (_cache.TryGetValue(cacheKey, out TopTypologiesDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Top Tipologie recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Cache 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Top Tipologie calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // var cacheKey = $"TopEarningsData_{userId}_{year ?? DateTime.UtcNow.Year}";

                // if (_cache.TryGetValue(cacheKey, out TopEarningsDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Top Guadagni recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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
                // Mostra SEMPRE tutti gli immobili attualmente in portafoglio, INDIPENDENTEMENTE dall'anno di inserimento
                // Il portafoglio non viene filtrato per anno - mostra lo stato attuale del portafoglio
                var portfolioPropsQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                !p.Sold &&
                                !p.Archived && // Escludi immobili cancellati
                                (p.AssignmentEnd == default(DateTime) || 
                                 p.AssignmentEnd == new DateTime(1, 1, 1) || 
                                 p.AssignmentEnd >= now)); // Incarico ancora valido
                // NOTA: NON filtrare per CreationDate.Year - il portafoglio mostra sempre lo stato attuale

                var portfolioProps = await portfolioPropsQuery.ToListAsync();

                var portfolioItems = portfolioProps
                    .Select(p => new TopEarningItemModel
                    {
                        Id = p.Id.ToString(),
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        UserFirstName = p.User?.FirstName ?? string.Empty,
                        Price = p.GetPriceToUse(),
                        EffectiveCommission = p.EffectiveCommission
                    })
                    .OrderByDescending(x => x.EffectiveCommission)
                    .Take(5)
                    .ToList();

                result.Portfolio = portfolioItems;
                result.TotalPortfolioCommission = (decimal)portfolioProps.Sum(p => p.EffectiveCommission);

                // ===== Vendite anno selezionato (Sold=true, venduti nell'anno = filtro) =====
                // Include solo immobili venduti nell'anno selezionato con incarico valido AL MOMENTO DELLA VENDITA
                var salesPropsQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(p => p.User)
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                p.Sold &&
                                !p.Archived); // Escludi immobili cancellati

                var salesProps = await salesPropsQuery.ToListAsync();

                // Filtra per anno di vendita e verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA
                var salesYearProps = salesProps.Where(p =>
                {
                    var soldDate = (p.UpdateDate != default(DateTime) && p.UpdateDate != new DateTime(1, 1, 1))
                        ? p.UpdateDate
                        : p.CreationDate;
                    
                    // Verifica che sia venduto nell'anno selezionato
                    if (soldDate.Year != targetYear)
                        return false;
                    
                    // Verifica che l'incarico fosse valido AL MOMENTO DELLA VENDITA (non oggi)
                    if (p.AssignmentEnd == default(DateTime) || p.AssignmentEnd == new DateTime(1, 1, 1))
                        return true; // Incarico senza scadenza, sempre valido
                    
                    // L'incarico deve essere valido al momento della vendita
                    return p.AssignmentEnd > soldDate;
                }).ToList();

                var salesItems = salesYearProps
                    .Select(p => new TopEarningItemModel
                    {
                        Id = p.Id.ToString(),
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        UserFirstName = p.User?.FirstName ?? string.Empty,
                        Price = p.GetPriceToUse(),
                        EffectiveCommission = p.EffectiveCommission
                    })
                    .OrderByDescending(x => x.EffectiveCommission)
                    .Take(5)
                    .ToList();

                result.SalesYear = salesItems;
                result.TotalSalesYearCommission = (decimal)salesYearProps.Sum(p => p.EffectiveCommission);

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Cache 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Top Guadagni calcolati e salvati in cache per chiave: {cacheKey}");

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
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId, year e agencyId
                // var cacheKey = $"AnalyticsData_{userId}_{year}_{agencyId ?? "all"}";

                // Verifica cache
                // if (_cache.TryGetValue(cacheKey, out AnalyticsDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati Analytics recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

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
                var nowDateOnly = DateTime.UtcNow.Date;
                
                var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) && 
                               circleUserIds.Contains(p.UserId) &&
                               // Escludi immobili scaduti (solo incarichi validi)
                               (p.AssignmentEnd == default(DateTime) || 
                                p.AssignmentEnd == new DateTime(1, 1, 1) || 
                                p.AssignmentEnd.Date >= nowDateOnly));

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

                // Appuntamenti confermati (per EventEndDate quando Confirmed == true)
                var appointmentsConfirmed = allAppointments
                    .Where(a => a.Confirmed && a.EventEndDate.Year == year)
                    .GroupBy(a => FormatMonthKey(a.EventEndDate.Month, year))
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

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Cache 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati Analytics calcolati e salvati in cache per chiave: {cacheKey}");

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

        public async Task<ExpiringAssignmentsDataModel> GetExpiringAssignments(string? userId, int? daysThreshold = 30)
        {
            try
            {
                var result = new ExpiringAssignmentsDataModel();
                var now = DateTime.UtcNow;
                var nowDateOnly = now.Date; // Solo la data, senza l'ora (inizio del giorno)
                var threshold = daysThreshold ?? 30;
                var thresholdDate = nowDateOnly.AddDays(threshold);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId non specificato per GetExpiringAssignments");
                    return result;
                }

                // Ottieni tutti gli userId nella cerchia dell'utente corrente
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Query per immobili non venduti con incarico in scadenza (entro 30 giorni, non ancora scaduti)
                var expiringProperties = await _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                !p.Sold && // Solo immobili non venduti
                                p.AssignmentEnd != default(DateTime) && // Deve avere una scadenza
                                p.AssignmentEnd != new DateTime(1, 1, 1) &&
                                p.AssignmentEnd.Date > nowDateOnly && // Non ancora scaduto (confronta solo la data)
                                p.AssignmentEnd.Date <= thresholdDate) // Scade entro la soglia
                    .OrderBy(p => p.AssignmentEnd) // Ordina per scadenza più imminente
                    .Select(p => new ExpiringAssignmentItemModel
                    {
                        Id = p.Id,
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        AssignmentEnd = p.AssignmentEnd,
                        DaysUntilExpiry = (int)Math.Ceiling((p.AssignmentEnd.Date - nowDateOnly).TotalDays)
                    })
                    .ToListAsync();

                result.Properties = expiringProperties;
                result.Total = expiringProperties.Count;

                // Query per immobili non venduti con incarico scaduto (tutti gli scaduti)
                // Considera scaduto se la data di scadenza è precedente a oggi (confronta solo la data)
                var expiredProperties = await _unitOfWork.dbContext.RealEstateProperties
                    .Where(p => !string.IsNullOrEmpty(p.UserId) &&
                                circleUserIds.Contains(p.UserId) &&
                                !p.Sold && // Solo immobili non venduti
                                p.AssignmentEnd != default(DateTime) && // Deve avere una scadenza
                                p.AssignmentEnd != new DateTime(1, 1, 1) &&
                                p.AssignmentEnd.Date < nowDateOnly) // Già scaduto (confronta solo la data, escludendo oggi)
                    .OrderByDescending(p => p.AssignmentEnd) // Ordina per scadenza più recente (più recenti prima)
                    .Select(p => new ExpiringAssignmentItemModel
                    {
                        Id = p.Id,
                        Title = p.Title ?? string.Empty,
                        AddressLine = p.AddressLine ?? string.Empty,
                        City = p.City ?? string.Empty,
                        AssignmentEnd = p.AssignmentEnd,
                        DaysUntilExpiry = (int)Math.Ceiling((p.AssignmentEnd.Date - nowDateOnly).TotalDays) // Negativo per scaduti
                    })
                    .ToListAsync();

                result.ExpiredProperties = expiredProperties;
                result.TotalExpired = expiredProperties.Count;

                _logger.LogInformation($"Trovati {result.Total} immobili con incarico in scadenza entro {threshold} giorni e {result.TotalExpired} scaduti per l'utente {userId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero degli immobili in scadenza: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero degli immobili in scadenza: {ex.Message}");
            }
        }

        public async Task<MatchedRequestsDataModel> GetMatchedRequests(string? userId)
        {
            try
            {
                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Genera chiave cache basata su userId
                // var cacheKey = $"MatchedRequests_{userId}";

                // Verifica cache
                // if (_cache.TryGetValue(cacheKey, out MatchedRequestsDataModel? cachedData))
                // {
                //     _logger.LogInformation($"Dati MatchedRequests recuperati dalla cache per chiave: {cacheKey}");
                //     return cachedData!;
                // }

                var result = new MatchedRequestsDataModel();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId non specificato per GetMatchedRequests");
                    return result;
                }

                // Ottieni tutti gli userId nella cerchia dell'utente
                var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                
                if (circleUserIds == null || !circleUserIds.Any())
                {
                    _logger.LogWarning($"Nessun userId trovato nella cerchia per l'utente {userId}");
                    return result;
                }

                // Recupera tutte le richieste nella cerchia (escludi chiuse e archiviate)
                var requestsQuery = _unitOfWork.dbContext.Requests
                    .Include(r => r.Customer)
                    .Where(r => r.UserId != null && 
                                circleUserIds.Contains(r.UserId) &&
                                !r.Closed && // Escludi richieste chiuse
                                !r.Archived); // Escludi richieste archiviate/cancellate

                var allRequests = await requestsQuery.ToListAsync();

                var matchedRequestsList = new List<MatchedRequestItemModel>();

                // Per ogni richiesta, trova il miglior match
                foreach (var request in allRequests)
                {
                    // Ottieni la cerchia estesa per questa richiesta (include Admin se Agency/Agent)
                    var extendedCircleIds = new List<string>(circleUserIds);
                    if (!string.IsNullOrEmpty(request.UserId))
                    {
                        var requestUser = await _userManager.FindByIdAsync(request.UserId);
                        if (requestUser != null)
                        {
                            var requestUserRoles = await _userManager.GetRolesAsync(requestUser);
                            
                            // Se è un'Agency, aggiungi anche l'Admin che l'ha creata
                            if (requestUserRoles.Contains("Agency") && !string.IsNullOrEmpty(requestUser.AdminId))
                            {
                                if (!extendedCircleIds.Contains(requestUser.AdminId))
                                    extendedCircleIds.Add(requestUser.AdminId);
                            }
                            // Se è un'Agent, aggiungi anche l'Admin (direttamente o tramite Agency)
                            else if (requestUserRoles.Contains("Agent") && !string.IsNullOrEmpty(requestUser.AdminId))
                            {
                                if (!extendedCircleIds.Contains(requestUser.AdminId))
                                    extendedCircleIds.Add(requestUser.AdminId);
                                
                                // Aggiungi anche l'Agency dell'agent (se esiste)
                                var agentAgency = await _userManager.FindByIdAsync(requestUser.AdminId);
                                if (agentAgency != null && !string.IsNullOrEmpty(agentAgency.AdminId))
                                {
                                    if (!extendedCircleIds.Contains(agentAgency.AdminId))
                                        extendedCircleIds.Add(agentAgency.AdminId);
                                }
                            }
                        }
                    }

                    // Filtro base: solo immobili non venduti nella cerchia estesa
                    var propertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(x => x.User)
                            .ThenInclude(u => u.Admin)
                        .Where(x => !x.Sold && 
                                   !string.IsNullOrEmpty(x.UserId) && 
                                   extendedCircleIds.Contains(x.UserId));

                    // Filtro City - se il comune non corrisponde, escludi
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        var requestCities = request.City.Split(',')
                            .Select(c => c.Trim().ToLower())
                            .Where(c => !string.IsNullOrEmpty(c))
                            .ToList();
                        
                        if (requestCities.Any())
                        {
                            propertiesQuery = propertiesQuery.Where(x => 
                                !string.IsNullOrEmpty(x.City) && 
                                requestCities.Contains(x.City.ToLower()));
                        }
                    }

                    var properties = await propertiesQuery.ToListAsync();
                    
                    // Calcola il match percentuale per ogni immobile
                    var propertiesWithMatch = new List<(RealEstateProperty Property, int MatchPercentage)>();
                    foreach (var property in properties)
                    {
                        int matchPercentage = CalculateMatchPercentage(request, property);
                        if (matchPercentage >= 60) // Solo match >= 60%
                        {
                            propertiesWithMatch.Add((property, matchPercentage));
                        }
                    }

                    // Se ci sono match, prendi il migliore (primo dopo ordinamento decrescente)
                    if (propertiesWithMatch.Any())
                    {
                        var bestMatch = propertiesWithMatch
                            .OrderByDescending(p => p.MatchPercentage)
                            .First();

                        matchedRequestsList.Add(new MatchedRequestItemModel
                        {
                            RequestId = request.Id,
                            CustomerLastName = request.Customer?.LastName ?? string.Empty,
                            CustomerName = request.Customer?.FirstName ?? string.Empty,
                            PropertyTitle = bestMatch.Property.Title ?? string.Empty,
                            MatchPercentage = bestMatch.MatchPercentage
                        });
                    }
                }

                // Ordina per match percentuale decrescente
                result.MatchedRequests = matchedRequestsList
                    .OrderByDescending(r => r.MatchPercentage)
                    .ToList();

                result.Total = result.MatchedRequests.Count;

                // CACHE DISABILITATA - Dati sempre aggiornati in tempo reale
                // Cache 5 minuti
                // var cacheOptions = new MemoryCacheEntryOptions
                // {
                //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                //     SlidingExpiration = TimeSpan.FromMinutes(2)
                // };
                // _cache.Set(cacheKey, result, cacheOptions);

                // _logger.LogInformation($"Dati MatchedRequests calcolati e salvati in cache per chiave: {cacheKey}, Total: {result.Total}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero dei dati MatchedRequests: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                throw new Exception($"Si è verificato un errore nel recupero dei dati MatchedRequests: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcola la percentuale di match tra una richiesta e un immobile
        /// Logica duplicata da RequestServices per coerenza
        /// </summary>
        private int CalculateMatchPercentage(Request request, RealEstateProperty property)
        {
            int totalCriteria = 0;
            int matchedCriteria = 0;

            // 1. Contract (Status) - OBBLIGATORIO
            totalCriteria++;
            if (property.Status != null && request.Contract != null && property.Status == request.Contract)
            {
                matchedCriteria++;
            }

            // 2. PropertyType (Typology) - se presente nella richiesta
            if (!string.IsNullOrEmpty(request.PropertyType))
            {
                totalCriteria++;
                var requestTypes = request.PropertyType.Split(',').Select(t => t.Trim().ToLower()).ToList();
                if (!string.IsNullOrEmpty(property.Typology))
                {
                    var propertyTypeLower = property.Typology.ToLower();
                    if (requestTypes.Any(type => propertyTypeLower.Contains(type)))
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 3. Province (State) - se presente nella richiesta
            if (!string.IsNullOrEmpty(request.Province))
            {
                totalCriteria++;
                if (!string.IsNullOrEmpty(property.State) && 
                    property.State.Equals(request.Province, StringComparison.OrdinalIgnoreCase))
                {
                    matchedCriteria++;
                }
            }

            // 4. City - già filtrato nella query, ma lo contiamo come match
            if (!string.IsNullOrEmpty(request.City))
            {
                totalCriteria++;
                var requestCities = request.City.Split(',').Select(c => c.Trim().ToLower()).ToList();
                if (!string.IsNullOrEmpty(property.City))
                {
                    var propertyCityLower = property.City.ToLower();
                    if (requestCities.Any(city => propertyCityLower.Contains(city)))
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 5. Price Range - se presente nella richiesta
            // Usa PriceReduced se > 0, altrimenti Price
            double propertyPriceToUse = (property.PriceReduced > 0) ? property.PriceReduced : property.Price;
            if (request.PriceFrom > 0 || request.PriceTo > 0)
            {
                totalCriteria++;
                if (request.PriceFrom > 0 && request.PriceTo > 0)
                {
                    if (propertyPriceToUse >= request.PriceFrom && propertyPriceToUse <= request.PriceTo)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.PriceFrom > 0)
                {
                    if (propertyPriceToUse >= request.PriceFrom)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.PriceTo > 0)
                {
                    if (propertyPriceToUse <= request.PriceTo)
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 6. MQ Range - se presente nella richiesta
            if (request.MQFrom > 0 || request.MQTo > 0)
            {
                totalCriteria++;
                if (request.MQFrom > 0 && request.MQTo > 0)
                {
                    if (property.CommercialSurfaceate > request.MQFrom && property.CommercialSurfaceate < request.MQTo)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.MQFrom > 0)
                {
                    if (property.CommercialSurfaceate > request.MQFrom)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.MQTo > 0)
                {
                    if (property.CommercialSurfaceate < request.MQTo)
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 7. Garden Range - se presente nella richiesta
            if (request.GardenFrom > 0 || request.GardenTo > 0)
            {
                totalCriteria++;
                if (request.GardenFrom > 0 && request.GardenTo > 0)
                {
                    if (property.MQGarden >= request.GardenFrom && property.MQGarden <= request.GardenTo)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.GardenFrom > 0)
                {
                    if (property.MQGarden >= request.GardenFrom)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.GardenTo > 0)
                {
                    if (property.MQGarden <= request.GardenTo)
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 8. PropertyState - se presente in entrambi
            if (!string.IsNullOrEmpty(request.PropertyState) && 
                !string.IsNullOrEmpty(property.StateOfTheProperty))
            {
                totalCriteria++;
                if (property.StateOfTheProperty == request.PropertyState)
                {
                    matchedCriteria++;
                }
            }

            // 9. Heating - se presente in entrambi
            if (!string.IsNullOrEmpty(request.Heating) && !string.IsNullOrEmpty(property.Heating))
            {
                totalCriteria++;
                if (property.Heating.Equals(request.Heating, StringComparison.OrdinalIgnoreCase))
                {
                    matchedCriteria++;
                }
            }

            // 10. ParkingSpaces - se presente nella richiesta
            if (request.ParkingSpaces > 0)
            {
                totalCriteria++;
                if (property.ParkingSpaces >= request.ParkingSpaces)
                {
                    matchedCriteria++;
                }
            }

            // 11. Rooms Range - se presente nella richiesta
            if (request.RoomsFrom > 0 || request.RoomsTo > 0)
            {
                totalCriteria++;
                if (request.RoomsFrom > 0 && request.RoomsTo > 0)
                {
                    if (property.Bedrooms >= request.RoomsFrom && property.Bedrooms <= request.RoomsTo)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.RoomsFrom > 0)
                {
                    if (property.Bedrooms >= request.RoomsFrom)
                    {
                        matchedCriteria++;
                    }
                }
                else if (request.RoomsTo > 0)
                {
                    if (property.Bedrooms <= request.RoomsTo)
                    {
                        matchedCriteria++;
                    }
                }
            }

            // 12. Bathrooms - se presente nella richiesta
            if (request.Bathrooms > 0)
            {
                totalCriteria++;
                if (property.Bathrooms >= request.Bathrooms)
                {
                    matchedCriteria++;
                }
            }

            // 13. Floor - se presente in entrambi
            if (!string.IsNullOrEmpty(request.Floor) && !string.IsNullOrEmpty(property.Floor))
            {
                totalCriteria++;
                if (property.Floor.Equals(request.Floor, StringComparison.OrdinalIgnoreCase))
                {
                    matchedCriteria++;
                }
            }

            // 14. Furniture - se presente in entrambi
            if (!string.IsNullOrEmpty(request.Furniture) && !string.IsNullOrEmpty(property.Furniture))
            {
                totalCriteria++;
                if (property.Furniture.Equals(request.Furniture, StringComparison.OrdinalIgnoreCase))
                {
                    matchedCriteria++;
                }
            }

            // 15. EnergyClass - se presente in entrambi
            if (!string.IsNullOrEmpty(request.EnergyClass) && !string.IsNullOrEmpty(property.EnergyClass))
            {
                totalCriteria++;
                if (property.EnergyClass == request.EnergyClass)
                {
                    matchedCriteria++;
                }
            }

            // 16. Auction - sempre confrontato
            totalCriteria++;
            if (property.Auction == request.Auction)
            {
                matchedCriteria++;
            }

            // Calcola la percentuale
            if (totalCriteria == 0)
                return 0;

            return (int)Math.Round((double)matchedCriteria / totalCriteria * 100);
        }
    }
}

