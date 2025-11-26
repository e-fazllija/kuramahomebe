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
                    var citys = item.City.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim().ToLower())
                                         .ToList();

                    var realEstatePropertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                        .Where(x =>
                            !x.Sold &&
                            x.Status == item.Contract &&
                            x.Price <= item.PriceTo &&
                            x.Price >= item.PriceFrom &&
                            citys.Any(t => x.City.ToLower().Contains(t)));


                    if (!string.IsNullOrEmpty(item.PropertyType))
                    {
                        realEstatePropertiesQuery.Where(x => (x.Typology ?? "").Contains(item.PropertyType));
                    }

                    if (!string.IsNullOrEmpty(item.RoomsNumber))
                    {
                        realEstatePropertiesQuery.Where(x => x.WarehouseRooms == Convert.ToInt32(item.RoomsNumber));
                    }

                    if (item.MQFrom > 0)
                    {
                        realEstatePropertiesQuery.Where(x => x.CommercialSurfaceate > item.MQFrom);
                    }

                    if (item.MQTo > 0)
                    {
                        realEstatePropertiesQuery.Where(x => x.CommercialSurfaceate < item.MQTo);
                    }

                    if (item.ParkingSpaces > 0)
                    {
                        realEstatePropertiesQuery.Where(x => x.ParkingSpaces >= item.ParkingSpaces);
                    }

                    List<RealEstateProperty> realEstateProperty = await realEstatePropertiesQuery.ToListAsync();
                    List<RealEstatePropertySelectModel> realEstatePropertySelectModel = _mapper.Map<List<RealEstatePropertySelectModel>>(realEstateProperty);
                    RequestSelectModel requestSelectModel = _mapper.Map<RequestSelectModel>(item);
                    requestSelectModel.RealEstateProperties = realEstatePropertySelectModel;
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
                        Location = x.Location,
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
                        Location = x.Location,
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

        public async Task<RequestSelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var request = await _unitOfWork.dbContext.Requests.Include(x => x.Customer).Include(x => x.RequestNotes)
                    //.Include(x => x.RequestType)
                    .FirstOrDefaultAsync(x => x.Id == id);

                var citys = request.City.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(t => t.Trim().ToLower())
                         .ToList();

                var realEstatePropertiesQuery = _unitOfWork.dbContext.RealEstateProperties
                    .Where(x =>
                        !x.Sold &&
                        x.Status == request.Contract &&
                        x.Price <= request.PriceTo &&
                        x.Price >= request.PriceFrom &&
                        citys.Any(t => x.City.ToLower().Contains(t)));


                if (!string.IsNullOrEmpty(request.PropertyType))
                {
                    realEstatePropertiesQuery.Where(x => (x.Typology ?? "").Contains(request.PropertyType));
                }

                if (!string.IsNullOrEmpty(request.RoomsNumber))
                {
                    realEstatePropertiesQuery.Where(x => x.WarehouseRooms == Convert.ToInt32(request.RoomsNumber));
                }

                if (request.MQFrom > 0)
                {
                    realEstatePropertiesQuery.Where(x => x.CommercialSurfaceate > request.MQFrom);
                }

                if (request.MQTo > 0)
                {
                    realEstatePropertiesQuery.Where(x => x.CommercialSurfaceate < request.MQTo);
                }

                if (request.ParkingSpaces > 0)
                {
                    realEstatePropertiesQuery.Where(x => x.ParkingSpaces >= request.ParkingSpaces);
                }

                List<RealEstateProperty> realEstateProperty = await realEstatePropertiesQuery.ToListAsync();
                List<RealEstatePropertySelectModel> realEstatePropertySelectModel = _mapper.Map<List<RealEstatePropertySelectModel>>(realEstateProperty);

                RequestSelectModel result = _mapper.Map<RequestSelectModel>(request);
                result.RealEstateProperties = new List<RealEstatePropertySelectModel>();
                result.RealEstateProperties?.AddRange(realEstatePropertySelectModel);

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
