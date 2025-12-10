using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.RequestModels;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using Microsoft.AspNetCore.Identity;
using BackEnd.Services;

namespace BackEnd.Services.BusinessServices
{
    public class RequestServices : IRequestServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RequestServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly AccessControlService _accessControl;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestServices(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RequestServices> logger,
            IOptionsMonitor<PaginationOptions> options,
            AccessControlService accessControl,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _accessControl = accessControl;
            _userManager = userManager;
        }
        public async Task<RequestSelectModel> Create(RequestCreateModel dto)
        {
            try
            {
                var entityClass = _mapper.Map<Request>(dto);
                
                // Imposta sempre CreationDate e UpdateDate in UTC quando si crea una nuova entità
                var now = DateTime.UtcNow;
                entityClass.CreationDate = now;
                entityClass.UpdateDate = now;
                
                await _unitOfWork.RequestRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                RequestSelectModel response = new RequestSelectModel();
                _mapper.Map(entityClass, response);

                _logger.LogInformation(nameof(Create));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task<Request> Delete(int id)
        {
            try
            {
                IQueryable<Request> query = _unitOfWork.dbContext.Requests;

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                Request EntityClasses = await query.FirstOrDefaultAsync();

                if (EntityClasses == null)
                    throw new NullReferenceException("Record non trovato!");

                // Elimina prima i Calendar collegati e le loro note
                var linkedCalendars = await _unitOfWork.dbContext.Calendars
                    .Where(x => x.RequestId == id)
                    .ToListAsync();

                if (linkedCalendars.Any())
                {
                    var calendarIds = linkedCalendars.Select(c => c.Id).ToList();

                    // Elimina le note collegate ai Calendar
                    var requestNotes = await _unitOfWork.dbContext.RequestNotes
                        .Where(x => x.CalendarId.HasValue && calendarIds.Contains(x.CalendarId.Value))
                        .ToListAsync();
                    if (requestNotes.Any())
                    {
                        _unitOfWork.dbContext.RequestNotes.RemoveRange(requestNotes);
                    }

                    var propertyNotes = await _unitOfWork.dbContext.RealEstatePropertyNotes
                        .Where(x => x.CalendarId.HasValue && calendarIds.Contains(x.CalendarId.Value))
                        .ToListAsync();
                    if (propertyNotes.Any())
                    {
                        _unitOfWork.dbContext.RealEstatePropertyNotes.RemoveRange(propertyNotes);
                    }

                    var customerNotes = await _unitOfWork.dbContext.CustomerNotes
                        .Where(x => x.CalendarId.HasValue && calendarIds.Contains(x.CalendarId.Value))
                        .ToListAsync();
                    if (customerNotes.Any())
                    {
                        _unitOfWork.dbContext.CustomerNotes.RemoveRange(customerNotes);
                    }

                    // Elimina i Calendar
                    _unitOfWork.dbContext.Calendars.RemoveRange(linkedCalendars);

                    // Salva le eliminazioni dei Calendar e delle note
                    await _unitOfWork.SaveAsync();
                }

                // Elimina la Request (le RequestNotes verranno eliminate automaticamente dal CASCADE del database)
                _unitOfWork.RequestRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation(nameof(Delete));

                return EntityClasses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione della richiesta con ID {id}: {ex.Message}");
                
                // Se è già un'eccezione con messaggio personalizzato, rilanciala
                if (ex.Message.Contains("Impossibile eliminare la richiesta"))
                {
                    throw;
                }

                // Gestione specifica per DbUpdateException (errori database)
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    if (dbEx.InnerException != null && 
                        (dbEx.InnerException.Message.Contains("DELETE statement conflicted") || 
                         dbEx.InnerException.Message.Contains("REFERENCE constraint")))
                    {
                        throw new Exception("Impossibile eliminare la richiesta perché è utilizzata come chiave esterna in un'altra tabella.");
                    }
                }

                // Gestione per InnerException (per compatibilità con codice esistente)
                if (ex.InnerException != null && 
                    ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare la richiesta perché è utilizzata come chiave esterna in un'altra tabella.");
                }

                // Gestione NullReferenceException
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }

                // Errore generico
                throw new Exception("Si è verificato un errore in fase di eliminazione. Riprova più tardi.");
            }
        }

        public async Task<ListViewModel<RequestSelectModel>> Get(int currentPage, string? filterRequest, char? fromName, char? toName, string? userId)
        {
            try
            {
                IQueryable<Request> query = _unitOfWork.dbContext.Requests.OrderByDescending(x => x.Id).Include(x => x.Customer);

                // Filtra per cerchia usando AccessControlService
                query = await ApplyRoleBasedFilter(query, userId);

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x => x.Customer.FirstName.Contains(filterRequest) || x.Customer.LastName.Contains(filterRequest));

                //if (fromName != null)
                //{
                //    string fromNameString = fromName.ToString();
                //    query = query.Where(x => string.Compare(x.Name.Substring(0, 1), fromNameString) >= 0);
                //}

                //if (toName != null)
                //{
                //    string toNameString = toName.ToString();
                //    query = query.Where(x => string.Compare(x.Name.Substring(0, 1), toNameString) <= 0);
                //}

                ListViewModel<RequestSelectModel> result = new ListViewModel<RequestSelectModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.AnagraficItemPerPage) - options.CurrentValue.AnagraficItemPerPage)
                            .Take(options.CurrentValue.AnagraficItemPerPage);
                }

                List<Request> queryList = await query
                    //.Include(x => x.RequestType)
                    .ToListAsync();

                result.Data = _mapper.Map<List<RequestSelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<ListViewModel<RequestSelectModel>> GetCustomerRequests(int customerId)
        {
            try
            {
                IQueryable<Request> query = _unitOfWork.dbContext.Requests.Include(x => x.Customer).Where(x => x.CustomerId == customerId).OrderByDescending(x => x.Id);
                List<Request> requests = await query.ToListAsync();

                ListViewModel<RequestSelectModel> result = new ListViewModel<RequestSelectModel>()
                {
                    Total = await query.CountAsync(),
                    Data = new List<RequestSelectModel>()
                };

                foreach (var item in requests)
                {
                    // Filtro base: solo immobili non venduti, includendo User e Admin per determinare agenzia
                    var realEstatePropertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Include(x => x.User)
                            .ThenInclude(u => u.Admin)
                        .Where(x => !x.Sold);

                    // Applica filtro per cerchia estesa dell'utente (include Admin, Agency e Agent)
                    if (!string.IsNullOrEmpty(item.UserId))
                    {
                        var circleUserIds = await GetExtendedCircleUserIdsForRequests(item.UserId);
                        realEstatePropertiesQuery = realEstatePropertiesQuery.Where(x => circleUserIds.Contains(x.UserId));
                    }

                    // FILTRO FONDAMENTALE: City - se il comune non corrisponde, escludi subito (ottimizzazione)
                    if (!string.IsNullOrEmpty(item.City))
                    {
                        var requestCities = item.City.Split(',')
                            .Select(c => c.Trim().ToLower())
                            .Where(c => !string.IsNullOrEmpty(c))
                            .ToList();
                        
                        if (requestCities.Any())
                        {
                            realEstatePropertiesQuery = realEstatePropertiesQuery.Where(x => 
                                !string.IsNullOrEmpty(x.City) && 
                                requestCities.Contains(x.City.ToLower()));
                        }
                    }

                    List<RealEstateProperty> realEstateProperties = await realEstatePropertiesQuery.ToListAsync();
                    
                    // Calcola il match percentuale per ogni immobile e filtra solo quelli >= 60%
                    var propertiesWithMatch = new List<RealEstatePropertySelectModel>();
                    foreach (var property in realEstateProperties)
                    {
                        int matchPercentage = CalculateMatchPercentage(item, property);
                        if (matchPercentage >= 60)
                        {
                            var propertyModel = _mapper.Map<RealEstatePropertySelectModel>(property);
                            propertyModel.MatchPercentage = matchPercentage;
                            
                            // Calcola agenzia di competenza
                            propertyModel.AgencyName = await GetAgencyNameForProperty(property);
                            
                            propertiesWithMatch.Add(propertyModel);
                        }
                    }

                    // Ordina per match percentuale decrescente (migliori match prima)
                    propertiesWithMatch = propertiesWithMatch.OrderByDescending(p => p.MatchPercentage).ToList();

                    RequestSelectModel requestSelectModel = _mapper.Map<RequestSelectModel>(item);
                    requestSelectModel.RealEstateProperties = propertiesWithMatch;
                    result.Data.Add(requestSelectModel);
                }

                _logger.LogInformation(nameof(GetCustomerRequests));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<ListViewModel<RequestListModel>> GetList(int currentPage, string? filterRequest, char? fromName, char? toName, string? userId)
        {
            try
            {
                IQueryable<Request> query = _unitOfWork.dbContext.Requests
                    .Include(x => x.Customer)
                    .OrderByDescending(x => x.Id);

                // Filtra per cerchia usando AccessControlService
                query = await ApplyRoleBasedFilter(query, userId);

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x => x.Customer.FirstName.Contains(filterRequest) || x.Customer.LastName.Contains(filterRequest));

                ListViewModel<RequestListModel> result = new ListViewModel<RequestListModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.AnagraficItemPerPage) - options.CurrentValue.AnagraficItemPerPage)
                            .Take(options.CurrentValue.AnagraficItemPerPage);
                }

                // Proiezione ottimizzata per la lista
                var queryList = await query
                    .Select(x => new RequestListModel
                    {
                        Id = x.Id,
                        CustomerName = x.Customer.FirstName,
                        CustomerLastName = x.Customer.LastName,
                        CustomerEmail = x.Customer.Email,
                        CustomerPhone = x.Customer.Phone.ToString(),
                        Contract = x.Contract,
                        CreationDate = x.CreationDate,
                        City = x.City,
                        PriceTo = x.PriceTo,
                        PriceFrom = x.PriceFrom,
                        PropertyType = x.PropertyType,
                        Archived = x.Archived,
                        Closed = x.Closed,
                        UserId = x.UserId
                    })
                    .ToListAsync();

                result.Data = queryList;

                _logger.LogInformation(nameof(GetList));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<List<RequestListModel>> GetForExportAsync(RequestExportModel filters, string userId)
        {
            try
            {
                filters ??= new RequestExportModel();

                IQueryable<Request> query = _unitOfWork.dbContext.Requests
                    .Include(x => x.Customer)
                    .OrderByDescending(x => x.Id);

                query = await ApplyRoleBasedFilter(query, userId);

                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var lowered = filters.Search.ToLower();
                    query = query.Where(x =>
                        x.Customer.FirstName.ToLower().Contains(lowered) ||
                        x.Customer.LastName.ToLower().Contains(lowered) ||
                        x.Customer.Email.ToLower().Contains(lowered));
                }

                if (filters.FromDate.HasValue)
                {
                    var from = DateTime.SpecifyKind(filters.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(x => x.CreationDate >= from);
                }

                if (filters.ToDate.HasValue)
                {
                    var to = DateTime.SpecifyKind(filters.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(x => x.CreationDate < to);
                }

                if (!string.IsNullOrEmpty(filters.Contract))
                {
                    query = query.Where(x => x.Contract == filters.Contract);
                }

                if (filters.PriceFrom.HasValue && filters.PriceFrom.Value > 0)
                {
                    query = query.Where(x => x.PriceFrom >= filters.PriceFrom.Value);
                }

                if (filters.PriceTo.HasValue && filters.PriceTo.Value > 0)
                {
                    query = query.Where(x => x.PriceTo <= filters.PriceTo.Value);
                }

                if (!string.IsNullOrEmpty(filters.Province))
                {
                    var provinceLower = filters.Province.ToLower();
                    query = query.Where(x => x.Province != null && x.Province.ToLower().Contains(provinceLower));
                }

                if (!string.IsNullOrEmpty(filters.City))
                {
                    var cityLower = filters.City.ToLower();
                    query = query.Where(x => x.City != null && x.City.ToLower().Contains(cityLower));
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    switch (filters.Status)
                    {
                        case "Aperta":
                            query = query.Where(x => !x.Archived && !x.Closed);
                            break;
                        case "Chiusa":
                            query = query.Where(x => x.Closed);
                            break;
                        case "Archiviata":
                            query = query.Where(x => x.Archived);
                            break;
                    }
                }

                if (filters.PropertyTypes != null && filters.PropertyTypes.Any())
                {
                    var loweredPropertyTypes = filters.PropertyTypes
                        .Where(pt => !string.IsNullOrWhiteSpace(pt))
                        .Select(pt => pt.ToLower())
                        .ToList();

                    if (loweredPropertyTypes.Any())
                    {
                        query = query.Where(x =>
                            !string.IsNullOrEmpty(x.PropertyType) &&
                            loweredPropertyTypes.Any(pt => x.PropertyType.ToLower().Contains(pt)));
                    }
                }

                var data = await query
                    .Select(x => new RequestListModel
                    {
                        Id = x.Id,
                        CustomerName = x.Customer.FirstName,
                        CustomerLastName = x.Customer.LastName,
                        CustomerEmail = x.Customer.Email,
                        CustomerPhone = x.Customer.Phone.ToString(),
                        Contract = x.Contract,
                        CreationDate = x.CreationDate,
                        City = x.City,
                        PriceTo = x.PriceTo,
                        PriceFrom = x.PriceFrom,
                        PropertyType = x.PropertyType,
                        Archived = x.Archived,
                        Closed = x.Closed,
                        UserId = x.UserId
                    })
                    .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore durante l'esportazione delle richieste");
            }
        }

        private async Task<IQueryable<Request>> ApplyRoleBasedFilter(IQueryable<Request> query, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return query;
            }

            var currentUser = await _userManager.FindByIdAsync(userId);

            if (currentUser == null)
            {
                return query.Where(request => false);
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
            return query.Where(request => request.UserId != null && circleUserIds.Contains(request.UserId));
        }

        /// <summary>
        /// Ottiene gli ID degli utenti nella cerchia estesa per le richieste.
        /// Include anche l'Admin per Agency e Agent, in modo che possano vedere tutti gli immobili della gerarchia.
        /// </summary>
        private async Task<List<string>> GetExtendedCircleUserIdsForRequests(string userId)
        {
            var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
            var extendedCircleIds = new List<string>(circleUserIds);

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return extendedCircleIds;

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            // Se è un'Agency, aggiungi anche l'Admin che l'ha creata
            if (currentUserRoles.Contains("Agency") && !string.IsNullOrEmpty(currentUser.AdminId))
            {
                extendedCircleIds.Add(currentUser.AdminId);
            }
            // Se è un'Agent, aggiungi anche l'Admin (direttamente o tramite Agency)
            else if (currentUserRoles.Contains("Agent") && !string.IsNullOrEmpty(currentUser.AdminId))
            {
                extendedCircleIds.Add(currentUser.AdminId);
                
                // Se l'Agent appartiene a un'Agency, verifica se l'Agency ha un Admin
                var agency = await _userManager.FindByIdAsync(currentUser.AdminId);
                if (agency != null)
                {
                    var agencyRoles = await _userManager.GetRolesAsync(agency);
                    // Se l'AdminId dell'Agent punta a un'Agency, e l'Agency ha un Admin, aggiungilo
                    if (agencyRoles.Contains("Agency") && !string.IsNullOrEmpty(agency.AdminId))
                    {
                        extendedCircleIds.Add(agency.AdminId);
                    }
                }
            }

            return extendedCircleIds.Distinct().ToList();
        }

        /// <summary>
        /// Determina il nome dell'agenzia di competenza per un immobile
        /// </summary>
        private async Task<string?> GetAgencyNameForProperty(RealEstateProperty property)
        {
            if (property.User == null)
                return null;

            var userRoles = await _userManager.GetRolesAsync(property.User);
            
            // Se è un Agent, l'agenzia di competenza è il suo Admin (Agency o Admin)
            if (userRoles.Contains("Agent"))
            {
                // Se Admin è già caricato, usalo
                if (property.User.Admin != null)
                {
                    if (!string.IsNullOrEmpty(property.User.Admin.CompanyName))
                        return property.User.Admin.CompanyName;
                    return $"{property.User.Admin.FirstName} {property.User.Admin.LastName}".Trim();
                }
                
                // Se Admin non è caricato ma AdminId esiste, caricalo
                if (!string.IsNullOrEmpty(property.User.AdminId))
                {
                    var admin = await _userManager.FindByIdAsync(property.User.AdminId);
                    if (admin != null)
                    {
                        if (!string.IsNullOrEmpty(admin.CompanyName))
                            return admin.CompanyName;
                        return $"{admin.FirstName} {admin.LastName}".Trim();
                    }
                }
                
                return null;
            }
            
            // Se è un'Agency, l'agenzia di competenza è l'Agency stessa
            if (userRoles.Contains("Agency"))
            {
                if (!string.IsNullOrEmpty(property.User.CompanyName))
                    return property.User.CompanyName;
                return $"{property.User.FirstName} {property.User.LastName}".Trim();
            }
            
            // Se è un Admin, l'agenzia di competenza è l'Admin stesso
            if (userRoles.Contains("Admin"))
            {
                if (!string.IsNullOrEmpty(property.User.CompanyName))
                    return property.User.CompanyName;
                return $"{property.User.FirstName} {property.User.LastName}".Trim();
            }

            return null;
        }

        /// <summary>
        /// Calcola la percentuale di match tra una richiesta e un immobile
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

            // 4. City - se presente nella richiesta
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
            double propertyPriceToUse = property.GetPriceToUse();
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

            // 16. Auction - se presente nella richiesta
            totalCriteria++;
            if (property.Auction == request.Auction)
            {
                matchedCriteria++;
            }

            // Calcola la percentuale
            if (totalCriteria == 0) return 0;
            return (int)Math.Round((double)matchedCriteria / totalCriteria * 100);
        }

        public async Task<RequestSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var request = await _unitOfWork.dbContext.Requests.Include(x => x.Customer).Include(x => x.RequestNotes)
                    //.Include(x => x.RequestType)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (request == null)
                    throw new Exception("Richiesta non trovata!");

                // Filtro base: solo immobili non venduti, includendo User e Admin per determinare agenzia
                var realEstatePropertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.User)
                        .ThenInclude(u => u.Admin)
                    .Where(x => !x.Sold);

                // Applica filtro per cerchia estesa dell'utente (include Admin, Agency e Agent)
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    var circleUserIds = await GetExtendedCircleUserIdsForRequests(request.UserId);
                    realEstatePropertiesQuery = realEstatePropertiesQuery.Where(x => circleUserIds.Contains(x.UserId));
                }

                // FILTRO FONDAMENTALE: City - se il comune non corrisponde, escludi subito (ottimizzazione)
                if (!string.IsNullOrEmpty(request.City))
                {
                    var requestCities = request.City.Split(',')
                        .Select(c => c.Trim().ToLower())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();
                    
                    if (requestCities.Any())
                    {
                        realEstatePropertiesQuery = realEstatePropertiesQuery.Where(x => 
                            !string.IsNullOrEmpty(x.City) && 
                            requestCities.Contains(x.City.ToLower()));
                    }
                }

                List<RealEstateProperty> realEstateProperties = await realEstatePropertiesQuery.ToListAsync();
                
                // Calcola il match percentuale per ogni immobile e filtra solo quelli >= 60%
                var propertiesWithMatch = new List<RealEstatePropertySelectModel>();
                foreach (var property in realEstateProperties)
                {
                    int matchPercentage = CalculateMatchPercentage(request, property);
                    if (matchPercentage >= 60)
                    {
                        var propertyModel = _mapper.Map<RealEstatePropertySelectModel>(property);
                        propertyModel.MatchPercentage = matchPercentage;
                        
                        // Calcola agenzia di competenza
                        propertyModel.AgencyName = await GetAgencyNameForProperty(property);
                        
                        propertiesWithMatch.Add(propertyModel);
                    }
                }

                // Ordina per match percentuale decrescente (migliori match prima)
                propertiesWithMatch = propertiesWithMatch.OrderByDescending(p => p.MatchPercentage).ToList();

                RequestSelectModel result = _mapper.Map<RequestSelectModel>(request);
                result.RealEstateProperties = propertiesWithMatch;

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<RequestSelectModel> Update(RequestUpdateModel dto)
        {
            try
            {
                var EntityClass =
                    await _unitOfWork.RequestRepository.FirstOrDefaultAsync(q => q.Where(x => x.Id == dto.Id));

                if (EntityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                EntityClass = _mapper.Map(dto, EntityClass);
                
                // Aggiorna sempre UpdateDate in UTC quando si modifica un'entità
                EntityClass.UpdateDate = DateTime.UtcNow;

                _unitOfWork.RequestRepository.Update(EntityClass);
                await _unitOfWork.SaveAsync();

                RequestSelectModel response = new RequestSelectModel();
                _mapper.Map(EntityClass, response);

                _logger.LogInformation(nameof(Update));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }
                else
                {
                    throw new Exception("Si è verificato un errore in fase di modifica");
                }
            }
        }
    }
}
