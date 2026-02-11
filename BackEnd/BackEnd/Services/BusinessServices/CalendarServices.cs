using AutoMapper;
using BackEnd.Entities;
using BackEnd.Exceptions;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CalendarModels;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RequestModels;
using BackEnd.Models.UserModel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage.File.Protocol;

namespace BackEnd.Services.BusinessServices
{
    public class CalendarServices : ICalendarServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CalendarServices> _logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AccessControlService _accessControl;
        
        public CalendarServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CalendarServices> logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AccessControlService accessControl)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _accessControl = accessControl;
        }

        public async Task<CalendarSelectModel> Create(CalendarCreateModel dto)
        {
            try
            {
                // Controllo sovrapposizione: stessa casa, stesso orario (solo se è associata una proprietà)
                if (dto.RealEstatePropertyId.HasValue && dto.RealEstatePropertyId.Value > 0)
                {
                    await EnsureNoOverlappingAppointmentForPropertyAsync(
                        dto.RealEstatePropertyId.Value,
                        dto.EventStartDate,
                        dto.EventEndDate,
                        excludeEventId: null);
                }

                //dto.DataInizioEvento = dto.DataInizioEvento.AddHours(1);
                //dto.DataFineEvento = dto.DataFineEvento.AddHours(1);
                var entityClass = _mapper.Map<Calendar>(dto);
                var result = await _unitOfWork.CalendarRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                ApplicationUser user = await userManager.FindByIdAsync(entityClass.UserId);

                if(entityClass.RequestId > 0 && entityClass.RequestId != null)
                {
                    RequestNotes note = new RequestNotes()
                    {
                        UserId = entityClass.UserId,
                        CalendarId = result.Entity.Id,
                        RequestId = entityClass.RequestId ?? 0,
                        Text = $"<strong>Nota di</strong>: {user.FirstName} {user.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };

                    await _unitOfWork.dbContext.RequestNotes.AddAsync(note);
                    _unitOfWork.Save();
                }

                if (entityClass.RealEstatePropertyId > 0 && entityClass.RealEstatePropertyId != null)
                {
                    RealEstatePropertyNotes note = new RealEstatePropertyNotes()
                    {
                        UserId = entityClass.UserId,
                        CalendarId = result.Entity.Id,
                        RealEstatePropertyId = entityClass.RealEstatePropertyId ?? 0,
                        Text = $"<strong>Nota di</strong>: {user.FirstName} {user.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };
                    await _unitOfWork.dbContext.RealEstatePropertyNotes.AddAsync(note);
                    _unitOfWork.Save();
                }

                if (entityClass.CustomerId > 0 && entityClass.CustomerId != null)
                {
                    CustomerNotes note = new CustomerNotes()
                    {
                        UserId = entityClass.UserId,
                        CalendarId = result.Entity.Id,
                        CustomerId = entityClass.CustomerId ?? 0,
                        Text = $"<strong>Nota di</strong>: {user.FirstName} {user.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };

                    await _unitOfWork.dbContext.CustomerNotes.AddAsync(note);
                    _unitOfWork.Save();
                }

                CalendarSelectModel response = new CalendarSelectModel();
                _mapper.Map(entityClass, response);

                _logger.LogInformation(nameof(Create));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex is CalendarOverlapException)
                    throw;
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task<Calendar> Delete(int id)
        {
            try
            {
                IQueryable<Calendar> query = _unitOfWork.dbContext.Calendars;

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                Calendar entityClass = await query.FirstOrDefaultAsync();

                if (entityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                if (entityClass.RequestId > 0 && entityClass.RequestId != null)
                {
                    RequestNotes? note = await _unitOfWork.dbContext.RequestNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
                    if(note != null)
                    {
                        _unitOfWork.dbContext.RequestNotes.Remove(note);
                        _unitOfWork.Save();
                    }
                }

                if (entityClass.RealEstatePropertyId > 0 && entityClass.RealEstatePropertyId != null)
                {
                    RealEstatePropertyNotes? note = await _unitOfWork.dbContext.RealEstatePropertyNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
                    if (note != null)
                    {
                        _unitOfWork.dbContext.RealEstatePropertyNotes.Remove(note);
                        _unitOfWork.Save();
                    } 
                }

                if (entityClass.CustomerId > 0 && entityClass.CustomerId != null)
                {
                    CustomerNotes? note = await _unitOfWork.dbContext.CustomerNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
                    if (note != null)
                    {
                        _unitOfWork.dbContext.CustomerNotes.Remove(note);
                        _unitOfWork.Save();
                    }
                }

                _unitOfWork.CalendarRepository.Delete(entityClass);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation(nameof(Delete));

                return entityClass;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione dell'evento con ID {id}: {ex.Message}");
                
                // Gestione specifica per DbUpdateException (errori database)
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    if (dbEx.InnerException != null && 
                        (dbEx.InnerException.Message.Contains("DELETE statement conflicted") || 
                         dbEx.InnerException.Message.Contains("REFERENCE constraint")))
                    {
                        throw new Exception("Impossibile eliminare l'evento perché è utilizzato come chiave esterna in un'altra tabella.");
                    }
                }

                // Gestione per InnerException (per compatibilità con codice esistente)
                if (ex.InnerException != null && 
                    ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare l'evento perché è utilizzato come chiave esterna in un'altra tabella.");
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

        public async Task<ListViewModel<CalendarSelectModel>> Get(string? userId, char? fromName, char? toName)
        {
            try
            {
                // Query base: tutti gli appuntamenti ordinati per data
                IQueryable<Calendar> query = _unitOfWork.dbContext.Calendars
                    .Include(x => x.User)
                    .OrderByDescending(x => x.EventStartDate);
                
                // Filtra per cerchia con regole specifiche per il calendario
                if (!string.IsNullOrEmpty(userId))
                {
                    var currentUser = await userManager.FindByIdAsync(userId);
                    if (currentUser != null)
                    {
                        var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                        List<string> allowedUserIds = new List<string> { userId };

                        if (currentUserRoles.Contains("Admin"))
                        {
                            // Admin vede tutto nella sua cerchia
                            var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                            allowedUserIds = circleUserIds;
                        }
                        else if (currentUserRoles.Contains("Agency"))
                        {
                            // Agency vede solo: propri eventi + eventi dei suoi Agent
                            var agents = await userManager.GetUsersInRoleAsync("Agent");
                            var myAgents = agents.Where(x => x.AdminId == userId);
                            allowedUserIds.AddRange(myAgents.Select(x => x.Id));
                        }
                        else if (currentUserRoles.Contains("Agent"))
                        {
                            // Agent vede solo: propri eventi + eventi della sua Agency
                            if (!string.IsNullOrEmpty(currentUser.AdminId))
                            {
                                allowedUserIds.Add(currentUser.AdminId);
                            }
                        }

                        query = query.Where(x => allowedUserIds.Contains(x.UserId));
                    }
                }

                if (fromName != null)
                {
                    string fromNameString = fromName.ToString();
                    query = query.Where(x => string.Compare(x.EventName.Substring(0, 1), fromNameString) >= 0);
                }

                if (toName != null)
                {
                    string toNameString = toName.ToString();
                    query = query.Where(x => string.Compare(x.EventName.Substring(0, 1), toNameString) <= 0);
                }

                ListViewModel<CalendarSelectModel> result = new ListViewModel<CalendarSelectModel>();

                result.Total = await query.CountAsync();

                
                List<Calendar> queryList = await query
                    .Include(x => x.User)
                    .AsNoTracking()
                    .ToListAsync();

                result.Data = _mapper.Map<List<CalendarSelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<CalendarCreateViewModel> GetToInsert(string userId)
        {
            try
            {
                // Ottieni gli utenti le cui entità possono essere associate a un appuntamento
                var allowedUserIds = await GetCalendarAllowedUserIdsAsync(userId);
                
                // Recupera solo entità degli utenti autorizzati
                List<Customer> customers = await _unitOfWork.dbContext.Customers
                    .Where(x => allowedUserIds.Contains(x.UserId))
                    .ToListAsync();
                    
                List<RealEstateProperty> properties = await _unitOfWork.dbContext.RealEstateProperties
                    .Where(x => allowedUserIds.Contains(x.UserId))
                    .ToListAsync();
                    
                List<Request> requests = await _unitOfWork.dbContext.Requests
                    .Where(x => allowedUserIds.Contains(x.UserId))
                    .ToListAsync();

                CalendarCreateViewModel result = new CalendarCreateViewModel();
                result.Customers = _mapper.Map<List<CustomerSelectModel>>(customers);
                result.RealEstateProperties = _mapper.Map<List<RealEstatePropertySelectModel>>(properties);
                result.Requests = _mapper.Map<List<RequestSelectModel>>(requests);

                _logger.LogInformation(nameof(GetToInsert));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<CalendarSearchModel> GetSearchItems(string userId, string? agencyId)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                List<UserSelectModel> agencies = new List<UserSelectModel>();
                List<UserSelectModel> agents = new List<UserSelectModel>();
                
                if(await userManager.IsInRoleAsync(user, "Admin"))
                {
                    // Admin vede solo le proprie Agency (dove AdminId == userId)
                    var agenciesList = await userManager.GetUsersInRoleAsync("Agency");
                    agenciesList = agenciesList.Where(x => x.AdminId == userId).ToList();
                    agencies = _mapper.Map<List<UserSelectModel>>(agenciesList);
                    
                    // Admin vede anche gli agenti
                    var agentsList = await userManager.GetUsersInRoleAsync("Agent");
                    if (!string.IsNullOrEmpty(agencyId))
                    {
                        // Se è selezionata un'agency, mostra solo gli agenti di quella agency
                        agentsList = agentsList.Where(x => x.AdminId == agencyId).ToList();
                    }
                    else
                    {
                        // Altrimenti mostra tutti gli agenti della cerchia (propri + delle agency)
                        var myAgencyIds = agenciesList.Select(x => x.Id).ToList();
                        agentsList = agentsList.Where(x => 
                            x.AdminId == userId || // Agenti diretti dell'Admin
                            myAgencyIds.Contains(x.AdminId) // Agenti delle Agency dell'Admin
                        ).ToList();
                    }
                    agents = _mapper.Map<List<UserSelectModel>>(agentsList);
                }

                if(await userManager.IsInRoleAsync(user, "Agency"))
                {
                    // Agency non vede altre agenzie nel filtro (solo i suoi agenti)
                    agencies = new List<UserSelectModel>();
                    
                    var agentsList = await userManager.GetUsersInRoleAsync("Agent");
                    agentsList = agentsList.Where(x => x.AdminId == userId).ToList();
                    agents = _mapper.Map<List<UserSelectModel>>(agentsList);
                }

                if(await userManager.IsInRoleAsync(user, "Agent"))
                {
                    // Agent non vede altre agenzie nel filtro (solo se stesso)
                    agencies = new List<UserSelectModel>();
                    agents = new List<UserSelectModel>();
                }

                CalendarSearchModel result = new CalendarSearchModel()
                {
                    Agencies = agencies,
                    Agents = agents
                };

                _logger.LogInformation(nameof(GetSearchItems));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<List<CalendarSelectModel>> GetForExportAsync(string userId, CalendarExportModel filters)
        {
            try
            {
                filters ??= new CalendarExportModel();

                IQueryable<Calendar> query = _unitOfWork.dbContext.Calendars
                    .Include(x => x.User)
                    .OrderByDescending(x => x.EventStartDate);

                if (!string.IsNullOrEmpty(userId))
                {
                    var currentUser = await userManager.FindByIdAsync(userId);
                    if (currentUser != null)
                    {
                        var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                        List<string> allowedUserIds = new List<string> { userId };

                        if (currentUserRoles.Contains("Admin"))
                        {
                            // Admin vede tutto nella sua cerchia
                            var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                            allowedUserIds = circleUserIds;
                        }
                        else if (currentUserRoles.Contains("Agency"))
                        {
                            // Agency vede solo: propri eventi + eventi dei suoi Agent
                            var agents = await userManager.GetUsersInRoleAsync("Agent");
                            var myAgents = agents.Where(x => x.AdminId == userId);
                            allowedUserIds.AddRange(myAgents.Select(x => x.Id));
                        }
                        else if (currentUserRoles.Contains("Agent"))
                        {
                            // Agent vede solo: propri eventi + eventi della sua Agency
                            if (!string.IsNullOrEmpty(currentUser.AdminId))
                            {
                                allowedUserIds.Add(currentUser.AdminId);
                            }
                        }

                        query = query.Where(x => allowedUserIds.Contains(x.UserId));
                    }
                }

                if (filters.FromDate.HasValue)
                {
                    var from = DateTime.SpecifyKind(filters.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(x => x.EventStartDate >= from);
                }

                if (filters.ToDate.HasValue)
                {
                    var to = DateTime.SpecifyKind(filters.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(x => x.EventStartDate < to);
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    switch (filters.Status.ToLower())
                    {
                        case "confirmed":
                            query = query.Where(x => x.Confirmed && !x.Cancelled && !x.Postponed);
                            break;
                        case "cancelled":
                            query = query.Where(x => x.Cancelled);
                            break;
                        case "postponed":
                            query = query.Where(x => x.Postponed);
                            break;
                        case "pending":
                            query = query.Where(x => !x.Confirmed && !x.Cancelled && !x.Postponed);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(filters.AgentId))
                {
                    query = query.Where(x => x.UserId == filters.AgentId);
                }

                if (!string.IsNullOrEmpty(filters.AgencyId))
                {
                    query = query.Where(x => x.User.AdminId == filters.AgencyId);
                }

                if (!string.IsNullOrEmpty(filters.Filter))
                {
                    var lowered = filters.Filter.ToLower();
                    query = query.Where(x =>
                        x.EventName.ToLower().Contains(lowered) ||
                        (x.EventDescription != null && x.EventDescription.ToLower().Contains(lowered)));
                }

                var data = await query.AsNoTracking().ToListAsync();
                return _mapper.Map<List<CalendarSelectModel>>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore durante l'esportazione del calendario");
            }
        }

        public async Task<CalendarSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var query = await _unitOfWork.dbContext.Calendars
                    //.Include(x => x.CalendarType)
                    .FirstOrDefaultAsync(x => x.Id == id);

                CalendarSelectModel result = _mapper.Map<CalendarSelectModel>(query);

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        private async Task HandleRequestNotes(Calendar entityClass, int? requestId)
        {
            var existingRequestNote = await _unitOfWork.dbContext.RequestNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
            if (requestId != null && (entityClass.RequestId == null || entityClass.RequestId != requestId))
            {
                if (existingRequestNote == null)
                {
                    existingRequestNote = new RequestNotes
                    {
                        UserId = entityClass.UserId,
                        CalendarId = entityClass.Id,
                        RequestId = requestId.Value,
                        Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };
                    await _unitOfWork.dbContext.RequestNotes.AddAsync(existingRequestNote);
                }
                else
                {
                    existingRequestNote.RequestId = requestId.Value;
                    existingRequestNote.Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}";
                    _unitOfWork.dbContext.RequestNotes.Update(existingRequestNote);
                }
            }
            else if (requestId == null && entityClass.RequestId != null && existingRequestNote != null)
            {
                _unitOfWork.dbContext.RequestNotes.Remove(existingRequestNote);
            }
        }

        private async Task HandleRealEstatePropertyNotes(Calendar entityClass, int? realEstatePropertyId)
        {
            var existingPropertyNote = await _unitOfWork.dbContext.RealEstatePropertyNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
            if (realEstatePropertyId > 0)
            {
                if (existingPropertyNote == null)
                {
                    existingPropertyNote = new RealEstatePropertyNotes
                    {
                        UserId = entityClass.UserId,
                        RealEstatePropertyId = realEstatePropertyId ?? 0,
                        CalendarId = entityClass.Id,
                        Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };
                    await _unitOfWork.dbContext.RealEstatePropertyNotes.AddAsync(existingPropertyNote);
                }
                else
                {
                    existingPropertyNote.Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}";
                    _unitOfWork.dbContext.RealEstatePropertyNotes.Update(existingPropertyNote);
                }
            }
            else if (existingPropertyNote != null)
            {
                _unitOfWork.dbContext.RealEstatePropertyNotes.Remove(existingPropertyNote);
            }
        }

        private async Task HandleCustomerNotes(Calendar entityClass, int? customerId)
        {
            var existingCustomerNote = await _unitOfWork.dbContext.CustomerNotes.FirstOrDefaultAsync(x => x.CalendarId == entityClass.Id);
            if (customerId > 0)
            {
                if (existingCustomerNote == null)
                {
                    existingCustomerNote = new CustomerNotes
                    {
                        UserId = entityClass.UserId,
                        CustomerId = customerId ?? 0,
                        CalendarId = entityClass.Id,
                        Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}"
                    };
                    await _unitOfWork.dbContext.CustomerNotes.AddAsync(existingCustomerNote);
                }
                else
                {
                    existingCustomerNote.Text = $"<strong>Nota di</strong>: {entityClass.User.FirstName} {entityClass.User.LastName} <br> <strong>Titolo</strong>: {entityClass.EventName}";
                    _unitOfWork.dbContext.CustomerNotes.Update(existingCustomerNote);
                }
            }
            else if (existingCustomerNote != null)
            {
                _unitOfWork.dbContext.CustomerNotes.Remove(existingCustomerNote);
            }
        }


        public async Task<CalendarSelectModel> Update(CalendarUpdateModel dto)
        {
            try
            {
                var entityClass =
                    await _unitOfWork.dbContext.Calendars.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (entityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                // Controllo sovrapposizione: stessa casa, stesso orario (solo se è associata una proprietà)
                if (dto.RealEstatePropertyId.HasValue && dto.RealEstatePropertyId.Value > 0)
                {
                    await EnsureNoOverlappingAppointmentForPropertyAsync(
                        dto.RealEstatePropertyId.Value,
                        dto.EventStartDate,
                        dto.EventEndDate,
                        excludeEventId: dto.Id);
                }

                //dto.DataInizioEvento = dto.DataInizioEvento.AddHours(1);
                //dto.DataFineEvento = dto.DataFineEvento.AddHours(1);

                await HandleRequestNotes(entityClass, dto.RequestId);
                await HandleRealEstatePropertyNotes(entityClass, dto.RealEstatePropertyId);
                await HandleCustomerNotes(entityClass, dto.CustomerId);

                entityClass = _mapper.Map(dto, entityClass);

                _unitOfWork.CalendarRepository.Update(entityClass);
                await _unitOfWork.SaveAsync();

                CalendarSelectModel response = new CalendarSelectModel();
                _mapper.Map(entityClass, response);

                _logger.LogInformation(nameof(Update));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex is CalendarOverlapException)
                    throw;
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

        /// <summary>
        /// Verifica che non esista già un appuntamento (non cancellato) per la stessa casa nello stesso orario.
        /// In caso di sovrapposizione solleva un'eccezione con messaggio chiaro: agente e agenzia (o solo agenzia) di chi ha l'appuntamento.
        /// </summary>
        private async Task EnsureNoOverlappingAppointmentForPropertyAsync(int realEstatePropertyId, DateTime eventStart, DateTime eventEnd, int? excludeEventId)
        {
            var query = _unitOfWork.dbContext.Calendars
                .Include(x => x.User)
                .ThenInclude(u => u!.Admin)
                .Where(x => x.RealEstatePropertyId == realEstatePropertyId
                    && !x.Cancelled
                    && x.EventStartDate < eventEnd
                    && x.EventEndDate > eventStart);

            if (excludeEventId.HasValue && excludeEventId.Value > 0)
                query = query.Where(x => x.Id != excludeEventId.Value);

            var existing = await query.AsNoTracking().FirstOrDefaultAsync();
            if (existing == null)
                return;

            var agentName = $"{existing.User?.FirstName ?? ""} {existing.User?.LastName ?? ""}".Trim();
            if (string.IsNullOrEmpty(agentName))
                agentName = "—";

            string existingInfo;
            if (!string.IsNullOrEmpty(existing.User?.AdminId) && existing.User?.Admin != null)
            {
                var agencyName = !string.IsNullOrEmpty(existing.User.Admin.CompanyName)
                    ? existing.User.Admin.CompanyName
                    : $"{existing.User.Admin.FirstName} {existing.User.Admin.LastName}".Trim();
                if (string.IsNullOrEmpty(agencyName))
                    agencyName = "—";
                existingInfo = $"Agente {agentName} – Agenzia {agencyName}";
            }
            else
            {
                existingInfo = $"Agenzia {agentName}";
            }

            throw new CalendarOverlapException(
                "È già presente un altro appuntamento per questa casa in questo orario." +
                Environment.NewLine + Environment.NewLine +
                "Appuntamento esistente: " + existingInfo + ".");
        }

        /// <summary>
        /// Ottiene gli ID degli utenti le cui entità possono essere associate a un appuntamento.
        /// Per gli Agent: solo proprie + Agency + colleghi stessa Agency.
        /// Per Admin e Agency: usa la cerchia completa.
        /// </summary>
        private async Task<List<string>> GetCalendarAllowedUserIdsAsync(string userId)
        {
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return new List<string> { userId };

            var currentUserRoles = await userManager.GetRolesAsync(currentUser);

            // Per gli Agent: logica ristretta - solo proprie + Agency + colleghi stessa Agency
            if (currentUserRoles.Contains("Agent"))
            {
                var allowedUserIds = new List<string> { userId }; // Proprie entità
                
                // Aggiungi l'Agency se esiste
                if (!string.IsNullOrEmpty(currentUser.AdminId))
                {
                    allowedUserIds.Add(currentUser.AdminId);
                    
                    // Aggiungi tutti gli Agent della stessa Agency (colleghi)
                    var agents = await userManager.GetUsersInRoleAsync("Agent");
                    var colleagues = agents.Where(x => x.AdminId == currentUser.AdminId && x.Id != userId);
                    allowedUserIds.AddRange(colleagues.Select(x => x.Id));
                }
                
                return allowedUserIds.Distinct().ToList();
            }
            else
            {
                // Per Admin e Agency: usa la cerchia completa
                return await _accessControl.GetCircleUserIdsFor(userId);
            }
        }

        /// <summary>
        /// Verifica se un'entità può essere associata a un appuntamento nel calendario.
        /// Per gli Agent: solo entità proprie, della loro Agency o di colleghi della stessa Agency.
        /// Per Admin e Agency: usa la cerchia completa.
        /// </summary>
        public async Task<bool> CanAssociateEntityToCalendar(string currentUserId, string entityCreatorId)
        {
            var allowedUserIds = await GetCalendarAllowedUserIdsAsync(currentUserId);
            return allowedUserIds.Contains(entityCreatorId);
        }
    }
}