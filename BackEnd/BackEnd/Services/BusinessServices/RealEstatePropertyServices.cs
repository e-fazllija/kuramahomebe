using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.CustomerModels;
using Microsoft.AspNetCore.Identity;
using BackEnd.Models.UserModel;
using BackEnd.Models.CalendarModels;
using BackEnd.Services;

namespace BackEnd.Services.BusinessServices
{
    public class RealEstatePropertyServices : IRealEstatePropertyServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RealEstatePropertyServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly IPropertyStorageService _propertyStorageService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AccessControlService _accessControl;
        
        public RealEstatePropertyServices(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RealEstatePropertyServices> logger,
            IOptionsMonitor<PaginationOptions> options,
            IPropertyStorageService propertyStorageService,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControl
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _propertyStorageService = propertyStorageService;
            this.userManager = userManager;
            _accessControl = accessControl;
        }
        public async Task<RealEstatePropertySelectModel> Create(RealEstatePropertyCreateModel dto)
        {
            try
            {
                // Converti tutte le DateTime del DTO a UTC prima del mapping
                // Questo è necessario perché PostgreSQL richiede DateTime con Kind=UTC
                if (dto.AssignmentEnd.Kind == DateTimeKind.Unspecified)
                {
                    dto.AssignmentEnd = DateTime.SpecifyKind(dto.AssignmentEnd, DateTimeKind.Utc);
                }
                else if (dto.AssignmentEnd.Kind == DateTimeKind.Local)
                {
                    dto.AssignmentEnd = dto.AssignmentEnd.ToUniversalTime();
                }
                
                // Verifica che AssignmentEnd sia valida, altrimenti imposta default
                if (dto.AssignmentEnd == default(DateTime) || dto.AssignmentEnd == DateTime.MinValue)
                {
                    dto.AssignmentEnd = DateTime.UtcNow.AddYears(1);
                }
                
                var entityClass = _mapper.Map<RealEstateProperty>(dto);
                
                // Imposta sempre CreationDate e UpdateDate in UTC quando si crea una nuova entità
                var now = DateTime.UtcNow;
                entityClass.CreationDate = now;
                entityClass.UpdateDate = now;
                
                var propertyAdded = await _unitOfWork.RealEstatePropertyRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                RealEstatePropertySelectModel response = new RealEstatePropertySelectModel();
                _mapper.Map(propertyAdded.Entity, response);

                _logger.LogInformation(nameof(Create));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella creazione dell'immobile nel servizio: {ex.Message}");
                
                // Log dell'inner exception se presente (per DbUpdateException)
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                
                // Propaga l'eccezione originale invece di crearne una nuova
                throw;
            }
        }

        public async Task InsertFiles(UploadFilesModel dto)
        {
            try
            {
                foreach (var file in dto.Files)
                {
                    Stream stream = file.OpenReadStream();
                    string fileName = $"RealEstatePropertyPhotos/{dto.PropertyId}/{file.FileName.Replace(" ", "-")}";
                    string fileUrl = await _propertyStorageService.UploadPropertyImage(stream, fileName);

                    var now = DateTime.UtcNow;
                    RealEstatePropertyPhoto photo = new RealEstatePropertyPhoto()
                    {
                        RealEstatePropertyId = dto.PropertyId,
                        FileName = fileName,
                        Url = fileUrl,
                        Type = 1,
                        CreationDate = now,
                        UpdateDate = now
                    };

                    await _unitOfWork.RealEstatePropertyPhotoRepository.InsertAsync(photo);
                    _unitOfWork.Save();
                }

                _logger.LogInformation(nameof(Create));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore in fase creazione");
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties.Include(x => x.Photos).Include(x => x.RealEstatePropertyNotes);

                if (id == 0)
                    throw new NullReferenceException("L'id non può essere 0");

                query = query.Where(x => x.Id == id);

                RealEstateProperty EntityClasses = await query.FirstOrDefaultAsync();

                if (EntityClasses == null)
                    throw new NullReferenceException("Record non trovato!");

                // Verifica preventiva se ci sono record collegati
                var hasEvents = await _unitOfWork.dbContext.Calendars.AnyAsync(x => x.RealEstatePropertyId == id);
                if (hasEvents)
                {
                    throw new Exception("Impossibile eliminare l'immobile perché è collegato a uno o più appuntamenti nel calendario.");
                }

                // Elimina le note collegate (se necessario)
                if (EntityClasses.RealEstatePropertyNotes != null && EntityClasses.RealEstatePropertyNotes.Count > 0)
                {
                    _unitOfWork.dbContext.RealEstatePropertyNotes.RemoveRange(EntityClasses.RealEstatePropertyNotes);
                    await _unitOfWork.SaveAsync();
                }

                // Elimina le foto prima di eliminare l'immobile
                foreach (var photo in EntityClasses.Photos)
                {
                    await _propertyStorageService.DeletePropertyImage(photo.FileName);
                }

                _unitOfWork.dbContext.RealEstatePropertyPhotos.RemoveRange(EntityClasses.Photos);
                await _unitOfWork.SaveAsync();

                _unitOfWork.RealEstatePropertyRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation(nameof(Delete));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione dell'immobile con ID {id}: {ex.Message}");
                
                // Se è già un'eccezione con messaggio personalizzato, rilanciala
                if (ex.Message.Contains("Impossibile eliminare l'immobile"))
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
                        throw new Exception("Impossibile eliminare l'immobile perché è utilizzato come chiave esterna in un'altra tabella.");
                    }
                }

                // Gestione per InnerException (per compatibilità con codice esistente)
                if (ex.InnerException != null && 
                    ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare l'immobile perché è utilizzato come chiave esterna in un'altra tabella.");
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

        public async Task<ListViewModel<RealEstatePropertySelectModel>> Get(
             int currentPage, string? filterRequest, string? status, string? typologie, string? location, int? code, int? from, int? to, string? agencyId, string? city, char? toName)
        {
            try
            {
                var nowDateOnly = DateTime.UtcNow.Date;
                
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                     .Include(x => x.Photos.OrderBy(x => x.Position))
                     .Where(x => !x.Archived && 
                                 x.User!.Admin!.EmailConfirmed &&
                                 // Escludi immobili scaduti (solo incarichi validi)
                                 (x.AssignmentEnd == default(DateTime) || 
                                  x.AssignmentEnd == new DateTime(1, 1, 1) || 
                                  x.AssignmentEnd.Date >= nowDateOnly))
                     //.Include(x => x.Agent)
                     .OrderByDescending(x => x.Id);

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x => x.AddressLine.Contains(filterRequest));

                if (!string.IsNullOrEmpty(status) && status != "Aste")
                    query = query.Where(x => x.Status.Contains(status) && !x.Auction);

                if (!string.IsNullOrEmpty(status) && status == "Aste")
                    query = query.Where(x => x.Auction);

                if (!string.IsNullOrEmpty(typologie) && typologie != "Qualsiasi")
                    query = query.Where(x => x.Typology!.ToLower().Contains(typologie.ToLower()));

                if (!string.IsNullOrEmpty(city) && city != "Qualsiasi")
                {
                    if (!string.IsNullOrEmpty(location) && location != "Qualsiasi")
                    {
                        // Se abbiamo sia city che location, cerchiamo per location specifica nella città
                        query = query.Where(x => x.City.ToLower() == city.ToLower() && x.Location.ToLower().Contains(location.ToLower()));
                    }
                    else
                    {
                        // Se abbiamo solo city, cerchiamo per città
                        query = query.Where(x => x.City.ToLower() == city.ToLower());
                    }
                }
                else if (!string.IsNullOrEmpty(location) && location != "Qualsiasi")
                {
                    // Se abbiamo solo location senza city, cerchiamo per località generica
                    query = query.Where(x => x.City.ToLower()!.Contains(location.ToLower()) || x.Location.ToLower().Contains(location.ToLower()));
                }

                if (code > 0)
                    query = query.Where(x => x.Id == code);
                if (from > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced >= from) || (x.PriceReduced <= 0 && x.Price >= from));
                if (to > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced <= to) || (x.PriceReduced <= 0 && x.Price <= to));
                if (!string.IsNullOrEmpty(agencyId))
                    query = query.Where(x => x.User.AdminId == agencyId);
                if (toName != null)
                {
                    string toNameString = toName.ToString();
                    query = query.Where(x => string.Compare(x.Category.Substring(0, 1), toNameString) <= 0);
                }
                ListViewModel<RealEstatePropertySelectModel> result = new ListViewModel<RealEstatePropertySelectModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.RealEstatePropertyItemPerPage) - options.CurrentValue.RealEstatePropertyItemPerPage)
                            .Take(options.CurrentValue.RealEstatePropertyItemPerPage);
                }

                List<RealEstateProperty> queryList = await query
                    //.Include(x => x.RealEstatePropertyType)
                    .ToListAsync();

                result.Data = _mapper.Map<List<RealEstatePropertySelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<ListViewModel<RealEstatePropertySelectModel>> Get(
            int currentPage, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city, string? userId = null)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.Photos.OrderBy(x => x.Position))
                    .OrderByDescending(x => x.Id);

                // Filtra per cerchia usando AccessControlService
                if (!string.IsNullOrEmpty(userId))
                {
                    var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                    query = query.Where(x => circleUserIds.Contains(x.UserId));
                }

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x =>
                    x.AddressLine.Contains(filterRequest) ||
                    x.Id.ToString().Contains(filterRequest));

                if (!string.IsNullOrEmpty(contract))
                {
                    if (contract == "Aste")
                    {
                        query = query.Where(x => x.Status == "Vendita" && x.Auction);
                    }
                    else
                    {
                        query = query.Where(x => x.Status == contract && !x.Auction);
                    }
                }

                if (priceFrom > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced >= priceFrom) || (x.PriceReduced <= 0 && x.Price >= priceFrom));

                if (priceTo > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced <= priceTo) || (x.PriceReduced <= 0 && x.Price <= priceTo));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(x => x.Category == category);

                if (!string.IsNullOrEmpty(typologie))
                    query = query.Where(x => x.Typology == typologie);

                if (!string.IsNullOrEmpty(city))
                {
                    var cityList = city.Split(",", StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim().ToLower())
                       .ToList();

                    query = query.Where(x => cityList.Contains(x.City.ToLower()));
                }

                ListViewModel<RealEstatePropertySelectModel> result = new ListViewModel<RealEstatePropertySelectModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.RealEstatePropertyItemPerPage) - options.CurrentValue.RealEstatePropertyItemPerPage)
                            .Take(options.CurrentValue.RealEstatePropertyItemPerPage);
                }

                List<RealEstateProperty> queryList = await query
                    //.Include(x => x.RealEstatePropertyType)
                    .ToListAsync();

                result.Data = _mapper.Map<List<RealEstatePropertySelectModel>>(queryList);

                _logger.LogInformation(nameof(Get));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public int GetPropertyCount()
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties;

                int total = query.Count();

                _logger.LogInformation(nameof(GetPropertyCount));

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<ListViewModel<RealEstatePropertyListModel>> GetList(int currentPage, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city, bool? sold, string? userId = null)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.Photos.OrderBy(x => x.Position))
                    .Include(x => x.User)
                    .OrderByDescending(x => x.Id);

                // Filtra per cerchia usando AccessControlService
                if (!string.IsNullOrEmpty(userId))
                {
                    var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                    query = query.Where(x => circleUserIds.Contains(x.UserId));
                }

                if (!string.IsNullOrEmpty(filterRequest))
                    query = query.Where(x =>
                    x.AddressLine.Contains(filterRequest) ||
                    x.Id.ToString().Contains(filterRequest));

                if (!string.IsNullOrEmpty(contract))
                {
                    if (contract == "Aste")
                    {
                        query = query.Where(x => x.Status == "Vendita" && x.Auction);
                    }
                    else
                    {
                        query = query.Where(x => x.Status == contract && !x.Auction);
                    }
                }

                if (priceFrom > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced >= priceFrom) || (x.PriceReduced <= 0 && x.Price >= priceFrom));

                if (priceTo > 0)
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced <= priceTo) || (x.PriceReduced <= 0 && x.Price <= priceTo));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(x => x.Category == category);

                if (!string.IsNullOrEmpty(typologie))
                    query = query.Where(x => x.Typology == typologie);

                if (!string.IsNullOrEmpty(city))
                {
                    var cityList = city.Split(",", StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim().ToLower())
                       .ToList();

                    query = query.Where(x => cityList.Contains(x.City.ToLower()));
                }

                // Filtro per immobili venduti
                if (sold.HasValue)
                {
                    query = query.Where(x => x.Sold == sold.Value);
                }

                ListViewModel<RealEstatePropertyListModel> result = new ListViewModel<RealEstatePropertyListModel>();

                result.Total = await query.CountAsync();

                if (currentPage > 0)
                {
                    query = query
                    .Skip((currentPage * options.CurrentValue.RealEstatePropertyItemPerPage) - options.CurrentValue.RealEstatePropertyItemPerPage)
                            .Take(options.CurrentValue.RealEstatePropertyItemPerPage);
                }

                // Proiezione ottimizzata per la lista
                var queryList = await query
                    .Select(x => new
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = x.Price,
                        PriceReduced = x.PriceReduced,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.Photos.OrderBy(p => p.Position).Select(p => p.Url).FirstOrDefault(),
                        AgencyId = x.User.AdminId,
                        AgentId = x.UserId,
                        AgreedCommission = x.AgreedCommission,
                        FlatRateCommission = x.FlatRateCommission,
                        CommissionReversal = x.CommissionReversal,
                        UpdateDate = x.UpdateDate
                    })
                    .ToListAsync();

                // Mappa e calcola EffectiveCommission
                var mappedList = queryList.Select(x =>
                {
                    double grossCommission = 0;
                    
                    // Usa PriceReduced se > 0, altrimenti Price
                    double priceToUse = PropertyPriceHelper.GetPriceToUse(x.Price, x.PriceReduced);
                    
                    // Calcola la provvigione lorda
                    if (x.AgreedCommission > 0 && priceToUse > 0)
                    {
                        grossCommission = (priceToUse * x.AgreedCommission) / 100.0;
                    }
                    else if (x.FlatRateCommission > 0)
                    {
                        grossCommission = x.FlatRateCommission;
                    }
                    
                    // Calcola la provvigione netta (lorda - storno)
                    double netCommission = grossCommission - x.CommissionReversal;
                    
                    // Il risultato non può essere negativo (minimo 0)
                    double effectiveCommission = Math.Max(0, netCommission);
                    
                    return new RealEstatePropertyListModel
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = priceToUse,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.FirstPhotoUrl,
                        AgencyId = x.AgencyId,
                        AgentId = x.AgentId,
                        EffectiveCommission = effectiveCommission,
                        UpdateDate = x.UpdateDate
                    };
                }).ToList();

                result.Data = mappedList;

                _logger.LogInformation(nameof(GetList));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<List<RealEstatePropertyListModel>> GetListForExportAsync(RealEstatePropertyExportModel filters, string userId)
        {
            try
            {
                filters ??= new RealEstatePropertyExportModel();

                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.Photos.OrderBy(p => p.Position))
                    .Include(x => x.User)
                    .OrderByDescending(x => x.Id);

                if (!string.IsNullOrEmpty(userId))
                {
                    var circleUserIds = await _accessControl.GetCircleUserIdsFor(userId);
                    query = query.Where(x => circleUserIds.Contains(x.UserId));
                }

                if (!string.IsNullOrEmpty(filters.Filter))
                {
                    var lowered = filters.Filter.ToLower();
                    query = query.Where(x =>
                        x.AddressLine.ToLower().Contains(lowered) ||
                        x.Id.ToString().Contains(lowered));
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
                    if (filters.Contract == "Aste")
                    {
                        query = query.Where(x => x.Status == "Vendita" && x.Auction);
                    }
                    else
                    {
                        query = query.Where(x => x.Status == filters.Contract && !x.Auction);
                    }
                }

                if (filters.PriceFrom.HasValue && filters.PriceFrom.Value > 0)
                {
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced >= filters.PriceFrom.Value) || (x.PriceReduced <= 0 && x.Price >= filters.PriceFrom.Value));
                }

                if (filters.PriceTo.HasValue && filters.PriceTo.Value > 0)
                {
                    // Filtra per prezzo: usa PriceReduced se > 0, altrimenti Price
                    query = query.Where(x => (x.PriceReduced > 0 && x.PriceReduced <= filters.PriceTo.Value) || (x.PriceReduced <= 0 && x.Price <= filters.PriceTo.Value));
                }

                if (!string.IsNullOrEmpty(filters.Category))
                {
                    query = query.Where(x => x.Category == filters.Category);
                }

                if (!string.IsNullOrEmpty(filters.Typologie))
                {
                    query = query.Where(x => x.Typology == filters.Typologie);
                }

                if (!string.IsNullOrEmpty(filters.City))
                {
                    var cityLower = filters.City.ToLower();
                    query = query.Where(x => x.City.ToLower().Contains(cityLower));
                }

                if (!string.IsNullOrEmpty(filters.Province))
                {
                    var provinceLower = filters.Province.ToLower();
                    query = query.Where(x => x.State != null && x.State.ToLower().Contains(provinceLower));
                }

                if (!string.IsNullOrEmpty(filters.AgentId))
                {
                    query = query.Where(x => x.UserId == filters.AgentId);
                }

                if (!string.IsNullOrEmpty(filters.AgencyId))
                {
                    query = query.Where(x => x.User.AdminId == filters.AgencyId);
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    query = query.Where(x => x.Status == filters.Status);
                }

                if (filters.Sold.HasValue)
                {
                    query = query.Where(x => x.Sold == filters.Sold.Value);
                }

                if (filters.Auction.HasValue)
                {
                    query = query.Where(x => x.Auction == filters.Auction.Value);
                }

                var queryList = await query
                    .Select(x => new
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = x.Price,
                        PriceReduced = x.PriceReduced,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.Photos.OrderBy(p => p.Position).Select(p => p.Url).FirstOrDefault(),
                        AgencyId = x.User.AdminId,
                        AgentId = x.UserId,
                        AgreedCommission = x.AgreedCommission,
                        FlatRateCommission = x.FlatRateCommission,
                        CommissionReversal = x.CommissionReversal,
                        UpdateDate = x.UpdateDate
                    })
                    .ToListAsync();

                // Mappa e calcola EffectiveCommission
                var data = queryList.Select(x =>
                {
                    double grossCommission = 0;
                    
                    // Usa PriceReduced se > 0, altrimenti Price
                    double priceToUse = PropertyPriceHelper.GetPriceToUse(x.Price, x.PriceReduced);
                    
                    // Calcola la provvigione lorda
                    if (x.AgreedCommission > 0 && priceToUse > 0)
                    {
                        grossCommission = (priceToUse * x.AgreedCommission) / 100.0;
                    }
                    else if (x.FlatRateCommission > 0)
                    {
                        grossCommission = x.FlatRateCommission;
                    }
                    
                    // Calcola la provvigione netta (lorda - storno)
                    double netCommission = grossCommission - x.CommissionReversal;
                    
                    // Il risultato non può essere negativo (minimo 0)
                    double effectiveCommission = Math.Max(0, netCommission);
                    
                    return new RealEstatePropertyListModel
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = priceToUse,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.FirstPhotoUrl,
                        AgencyId = x.AgencyId,
                        AgentId = x.AgentId,
                        EffectiveCommission = effectiveCommission,
                        UpdateDate = x.UpdateDate
                    };
                }).ToList();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore durante l'esportazione degli immobili");
            }
        }

        public async Task<RealEstatePropertyCreateViewModel> GetToInsert(string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    throw new ArgumentException("Utente non valido per il recupero dei dati di inserimento", nameof(currentUserId));
                }

                var currentUser = await userManager.FindByIdAsync(currentUserId)
                    ?? throw new InvalidOperationException("Utente corrente non trovato");

                var accessibleUserIds = await _accessControl.GetCircleUserIdsFor(currentUserId);
                if (!accessibleUserIds.Contains(currentUserId))
                {
                    accessibleUserIds.Add(currentUserId);
                }

                var accessibleUserIdsSet = accessibleUserIds.ToHashSet();

                // Gli AdminId di riferimento includono tutte le entità raggiungibili e l'eventuale superiore diretto.
                var accessibleAdminIds = new HashSet<string>(accessibleUserIdsSet);
                if (!string.IsNullOrEmpty(currentUser.AdminId))
                {
                    accessibleAdminIds.Add(currentUser.AdminId);
                }

                IQueryable<Customer> customerQuery = _unitOfWork.dbContext.Customers
                    .Include(c => c.User)
                    .Where(customer =>
                        !string.IsNullOrEmpty(customer.UserId) &&
                        accessibleUserIdsSet.Contains(customer.UserId));

                var agentsInCircle = (await userManager.GetUsersInRoleAsync("Agent"))
                    .Where(agent =>
                        !string.IsNullOrEmpty(agent.AdminId) &&
                        accessibleAdminIds.Contains(agent.AdminId))
                    .ToList();

                var agentModels = _mapper.Map<List<UserSelectModel>>(agentsInCircle);

                var currentUserModel = _mapper.Map<UserSelectModel>(currentUser);
                if (currentUserModel != null)
                {
                    agentModels.RemoveAll(agent => agent.Id == currentUserModel.Id);
                    agentModels.Insert(0, currentUserModel);
                }

                var result = new RealEstatePropertyCreateViewModel
                {
                    Customers = _mapper.Map<List<CustomerSelectModel>>(await customerQuery.ToListAsync()),
                    Agents = agentModels
                };

                _logger.LogInformation(nameof(GetToInsert));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task SetHighlighted(int realEstatePropertyId)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Id == realEstatePropertyId);
                IQueryable<RealEstateProperty> queryHighlighted = _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Highlighted == true);

                RealEstateProperty propertyHighlighted = await query.FirstAsync();
                propertyHighlighted.Highlighted = false;
                _unitOfWork.dbContext.RealEstateProperties.Update(propertyHighlighted);
                await _unitOfWork.SaveAsync();

                RealEstateProperty property = await query.FirstAsync();
                property.Highlighted = true;
                _unitOfWork.dbContext.RealEstateProperties.Update(property);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation(nameof(SetHighlighted));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task SetInHome(int realEstatePropertyId)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Id == realEstatePropertyId);

                RealEstateProperty property = await query.FirstAsync();
                property.InHome = true;
                _unitOfWork.dbContext.RealEstateProperties.Update(property);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation(nameof(SetInHome));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<RealEstatePropertySelectModel> GetById(int id)
        {
            try
            {
                if (id is not > 0)
                    throw new Exception("Si è verificato un errore!");

                var query = await _unitOfWork.dbContext.RealEstateProperties.Include(x => x.Photos.OrderBy(y => y.Position)).Include(x => x.User).Include(x => x.Customer)
                    .Include(x => x.RealEstatePropertyNotes)
                    .FirstOrDefaultAsync(x => x.Id == id);

                RealEstatePropertySelectModel result = _mapper.Map<RealEstatePropertySelectModel>(query);

                _logger.LogInformation(nameof(GetById));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<RealEstatePropertySelectModel> Update(RealEstatePropertyUpdateModel dto)
        {
            try
            {
                var EntityClass =
                    await _unitOfWork.RealEstatePropertyRepository.FirstOrDefaultAsync(q => q.Where(x => x.Id == dto.Id));

                if (EntityClass == null)
                    throw new NullReferenceException("Record non trovato!");

                // Converti AssignmentEnd a UTC se necessario (prima del mapping)
                if (dto.AssignmentEnd.Kind == DateTimeKind.Unspecified)
                {
                    dto.AssignmentEnd = DateTime.SpecifyKind(dto.AssignmentEnd, DateTimeKind.Utc);
                }
                else if (dto.AssignmentEnd.Kind == DateTimeKind.Local)
                {
                    dto.AssignmentEnd = dto.AssignmentEnd.ToUniversalTime();
                }

                EntityClass = _mapper.Map(dto, EntityClass);
                
                // Aggiorna sempre UpdateDate in UTC quando si modifica un'entità
                EntityClass.UpdateDate = DateTime.UtcNow;

                _unitOfWork.RealEstatePropertyRepository.Update(EntityClass);
                await _unitOfWork.SaveAsync();

                RealEstatePropertySelectModel response = new RealEstatePropertySelectModel();
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

        public async Task<CalendarSearchModel> GetSearchItems(string userId, string? agencyId)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                List<UserSelectModel> agencies = new List<UserSelectModel>();
                List<UserSelectModel> agents = new List<UserSelectModel>();

                var agenciesList = await userManager.GetUsersInRoleAsync("Agency");
                agencies = _mapper.Map<List<UserSelectModel>>(agenciesList);



                var agentsList = await userManager.GetUsersInRoleAsync("Agent");
                agentsList = agentsList.Where(x => x.AdminId == (agencyId ?? userId)).ToList();
                agents = _mapper.Map<List<UserSelectModel>>(agentsList);


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
    }
}
