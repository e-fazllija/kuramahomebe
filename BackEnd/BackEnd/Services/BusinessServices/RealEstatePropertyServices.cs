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

namespace BackEnd.Services.BusinessServices
{
    public class RealEstatePropertyServices : IRealEstatePropertyServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RealEstatePropertyServices> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly IStorageServices _storageServices;
        private readonly UserManager<ApplicationUser> userManager;
        public RealEstatePropertyServices(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RealEstatePropertyServices> logger,
            IOptionsMonitor<PaginationOptions> options,
            IStorageServices storageServices,
            UserManager<ApplicationUser> userManager
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            _storageServices = storageServices;
            this.userManager = userManager;
        }
        public async Task<RealEstatePropertySelectModel> Create(RealEstatePropertyCreateModel dto)
        {
            try
            {
                var entityClass = _mapper.Map<RealEstateProperty>(dto);
                var propertyAdded = await _unitOfWork.RealEstatePropertyRepository.InsertAsync(entityClass);
                _unitOfWork.Save();

                if (dto.Files?.Count > 0)
                {
                    foreach (var file in dto.Files)
                    {
                        Stream stream = file.OpenReadStream();
                        string fileName = $"RealEstatePropertyPhotos/{propertyAdded.Entity.Id}/{file.FileName.Replace(" ", "-")}";
                        string fileUrl = await _storageServices.UploadFile(stream, fileName);

                        RealEstatePropertyPhoto photo = new RealEstatePropertyPhoto()
                        {
                            RealEstatePropertyId = propertyAdded.Entity.Id,
                            FileName = fileName,
                            Url = fileUrl,
                            Type = 1
                        };

                        await _unitOfWork.RealEstatePropertyPhotoRepository.InsertAsync(photo);
                        _unitOfWork.Save();
                    }

                }

                RealEstatePropertySelectModel response = new RealEstatePropertySelectModel();
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

        public async Task InsertFiles(UploadFilesModel dto)
        {
            try
            {
                foreach (var file in dto.Files)
                {
                    Stream stream = file.OpenReadStream();
                    string fileName = $"RealEstatePropertyPhotos/{dto.PropertyId}/{file.FileName.Replace(" ", "-")}";
                    string fileUrl = await _storageServices.UploadFile(stream, fileName);

                    RealEstatePropertyPhoto photo = new RealEstatePropertyPhoto()
                    {
                        RealEstatePropertyId = dto.PropertyId,
                        FileName = fileName,
                        Url = fileUrl,
                        Type = 1
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

                //if (EntityClasses.RealEstatePropertyNotes != null && EntityClasses.RealEstatePropertyNotes?.Count > 0)
                //{
                //    _unitOfWork.dbContext.RealEstatePropertyNotes.RemoveRange(EntityClasses.RealEstatePropertyNotes);
                //    await _unitOfWork.SaveAsync();
                //}

                _unitOfWork.RealEstatePropertyRepository.Delete(EntityClasses);
                await _unitOfWork.SaveAsync();

                foreach (var photo in EntityClasses.Photos)
                {
                    await _storageServices.DeleteFile(photo.FileName);
                }

                _unitOfWork.dbContext.RealEstatePropertyPhotos.RemoveRange(EntityClasses.Photos);

                _logger.LogInformation(nameof(Delete));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    throw new Exception("Impossibile eliminare il record perché è utilizzato come chiave esterna in un'altra tabella.");
                }
                if (ex is NullReferenceException)
                {
                    throw new Exception(ex.Message);
                }
                else
                {
                    throw new Exception("Si è verificato un errore in fase di eliminazione");
                }
            }
        }

        public async Task<ListViewModel<RealEstatePropertySelectModel>> Get(
             int currentPage, string? filterRequest, string? status, string? typologie, string? location, int? code, int? from, int? to, string? agencyId, string? city, char? toName)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                     .Include(x => x.Photos.OrderBy(x => x.Position)).Where(x => !x.Archived && x.Agent!.Agency!.EmailConfirmed)
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
                        query = query.Where(x => x.Town.ToLower() == city.ToLower() && x.Location.ToLower().Contains(location.ToLower()));
                    }
                    else
                    {
                        // Se abbiamo solo city, cerchiamo per città
                        query = query.Where(x => x.Town.ToLower() == city.ToLower());
                    }
                }
                else if (!string.IsNullOrEmpty(location) && location != "Qualsiasi")
                {
                    // Se abbiamo solo location senza city, cerchiamo per località generica
                    query = query.Where(x => x.Town.ToLower()!.Contains(location.ToLower()) || x.Location.ToLower().Contains(location.ToLower()));
                }

                if (code > 0)
                    query = query.Where(x => x.Id == code);
                if (from > 0)
                    query = query.Where(x => x.Price >= from);
                if (to > 0)
                    query = query.Where(x => x.Price <= to);
                if (!string.IsNullOrEmpty(agencyId))
                    query = query.Where(x => x.Agent.AgencyId == agencyId);
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
            int currentPage, string? agencyId, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? town)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.Photos.OrderBy(x => x.Position))
                    .OrderByDescending(x => x.Id);

                if (!string.IsNullOrEmpty(agencyId) && agencyId != "all")
                    query = query.Where(x => x.Agent.AgencyId!.Contains(agencyId));

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
                    query = query.Where(x => x.Price >= priceFrom);

                if (priceTo > 0)
                    query = query.Where(x => x.Price <= priceTo);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(x => x.Category == category);

                if (!string.IsNullOrEmpty(typologie))
                    query = query.Where(x => x.Typology == typologie);

                if (!string.IsNullOrEmpty(town))
                {
                    var townList = town.Split(",", StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim().ToLower())
                       .ToList();

                    query = query.Where(x => townList.Contains(x.Town.ToLower()));
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

        public async Task<ListViewModel<RealEstatePropertyListModel>> GetList(int currentPage, string? agencyId, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? town, bool? sold)
        {
            try
            {
                IQueryable<RealEstateProperty> query = _unitOfWork.dbContext.RealEstateProperties
                    .Include(x => x.Photos.OrderBy(x => x.Position))
                    .Include(x => x.Agent)
                    .OrderByDescending(x => x.Id);

                if (!string.IsNullOrEmpty(agencyId) && agencyId != "all")
                    query = query.Where(x => x.Agent.AgencyId!.Contains(agencyId));

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
                    query = query.Where(x => x.Price >= priceFrom);

                if (priceTo > 0)
                    query = query.Where(x => x.Price <= priceTo);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(x => x.Category == category);

                if (!string.IsNullOrEmpty(typologie))
                    query = query.Where(x => x.Typology == typologie);

                if (!string.IsNullOrEmpty(town))
                {
                    var townList = town.Split(",", StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim().ToLower())
                       .ToList();

                    query = query.Where(x => townList.Contains(x.Town.ToLower()));
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
                    .Select(x => new RealEstatePropertyListModel
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        Town = x.Town,
                        Region = x.State,
                        Price = x.Price,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.Photos.OrderBy(p => p.Position).Select(p => p.Url).FirstOrDefault(),
                        AgencyId = x.Agent.AgencyId,
                        AgentId = x.AgentId
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

        public async Task<RealEstatePropertyCreateViewModel> GetToInsert(string? agencyId)
        {
            try
            {
                IQueryable<Customer> customerQuery = _unitOfWork.dbContext.Customers;
                var usersList = await userManager.GetUsersInRoleAsync("Agent");

                if (!string.IsNullOrEmpty(agencyId))
                    usersList = usersList.Where(x => x.AgencyId == agencyId).ToList();

                if (!string.IsNullOrEmpty(agencyId))
                    customerQuery = customerQuery.Where(x => x.AgencyId == agencyId);

                List<ApplicationUser> users = usersList.ToList();

                RealEstatePropertyCreateViewModel result = new RealEstatePropertyCreateViewModel();
                List<Customer> customers = await customerQuery.ToListAsync();
                result.Customers = _mapper.Map<List<CustomerSelectModel>>(customers);
                result.Agents = _mapper.Map<List<UserSelectModel>>(users);

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

                var query = await _unitOfWork.dbContext.RealEstateProperties.Include(x => x.Photos.OrderBy(y => y.Position)).Include(x => x.Agent).Include(x => x.Customer)
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

                EntityClass = _mapper.Map(dto, EntityClass);

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
                agentsList = agentsList.Where(x => x.AgencyId == (agencyId ?? userId)).ToList();
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
