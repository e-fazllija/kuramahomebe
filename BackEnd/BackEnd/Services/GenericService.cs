using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RealEstatePropertyPhotoModels;
using BackEnd.Services.BusinessServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities;
using System;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.CalendarModels;
using BackEnd.Models.UserModel;
using BackEnd.Models.RequestModels;

namespace BackEnd.Services
{
    public class GenericService : IGenericService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GenericService> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly UserManager<ApplicationUser> userManager;
        
        public GenericService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GenericService> logger, IOptionsMonitor<PaginationOptions> options, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            this.userManager = userManager;
        }

        public async Task<HomeDetailsModel> GetHomeDetails()
        {
            try
            {
                HomeDetailsModel result = new HomeDetailsModel();
                List<RealEstateProperty> propertiesInHome = await _unitOfWork.dbContext.RealEstateProperties.Where(x => x.InHome).Include(x => x.Photos.OrderBy(x => x.Position)).OrderByDescending(x => x.Id).ToListAsync();

                RealEstateProperty? propertyHighlighted =
                    await _unitOfWork.dbContext.RealEstateProperties.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Highlighted) ?? propertiesInHome.FirstOrDefault();

                result.RealEstatePropertiesHighlighted = _mapper.Map<RealEstatePropertySelectModel>(propertyHighlighted);

                if (result.RealEstatePropertiesHighlighted.Photos.Any(x => x.Highlighted))
                {
                    List<RealEstatePropertyPhotoSelectModel> photos = new List<RealEstatePropertyPhotoSelectModel>();
                    photos.Insert(0, result.RealEstatePropertiesHighlighted.Photos.FirstOrDefault(x => x.Highlighted)!);

                    result.RealEstatePropertiesHighlighted.Photos.Remove(result.RealEstatePropertiesHighlighted.Photos.FirstOrDefault(x => x.Highlighted)!);
                    photos.AddRange(result.RealEstatePropertiesHighlighted.Photos);
                    result.RealEstatePropertiesHighlighted.Photos = photos;
                }

                foreach (RealEstateProperty property in propertiesInHome)
                {
                    if (property.Photos.Any(x => x.Highlighted))
                    {
                        List<RealEstatePropertyPhoto> photos = new List<RealEstatePropertyPhoto>();
                        photos.Insert(0, property.Photos.FirstOrDefault(x => x.Highlighted)!);

                        property.Photos.Remove(property.Photos.FirstOrDefault(x => x.Highlighted)!);
                        photos.AddRange(property.Photos);
                        property.Photos = photos;
                    }
                }

                result.RealEstatePropertiesInHome = _mapper.Map<List<RealEstatePropertySelectModel>>(propertiesInHome);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<AdminHomeDetailsModel> GetAdminHomeDetails(string? agencyId)
        {
            try
            {
                AdminHomeDetailsModel result = new AdminHomeDetailsModel();
                IQueryable<RealEstateProperty> propertiesInHome = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.RealEstateProperties 
                    : _unitOfWork.dbContext.RealEstateProperties.Where(x => x.User.AdminId == agencyId);

                result.RealEstatePropertyHomeDetails.Total = propertiesInHome.Count();
                result.RealEstatePropertyHomeDetails.TotalSale = propertiesInHome.Where(x => x.Status == "Vendita").Count();
                result.RealEstatePropertyHomeDetails.TotalRent = propertiesInHome.Where(x => x.Status == "Affitto").Count();
                result.RealEstatePropertyHomeDetails.TotalLastMonth = propertiesInHome.Where(x => x.CreationDate >= DateTime.UtcNow.AddMonths(-1)).Count();

                foreach (var item in propertiesInHome.Where(x => x.Status == "Vendita"))
                {
                    if (!result.RealEstatePropertyHomeDetails.DistinctByCitySale.ContainsKey(item.City))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByCitySale[item.City] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByCitySale[item.City]++;
                    }

                    //By type
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTypeSale.ContainsKey(item.Typology ?? item.Category))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTypeSale[item.Typology ?? item.Category] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTypeSale[item.Typology ?? item.Category]++;
                    }
                }

                foreach (var item in propertiesInHome.Where(x => x.Status == "Affitto"))
                {
                    if (!result.RealEstatePropertyHomeDetails.DistinctByCityRent.ContainsKey(item.City))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByCityRent[item.City] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByCityRent[item.City]++;
                    }

                    //By type
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTypeRent.ContainsKey(item.Typology ?? item.Category))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTypeRent[item.Typology ?? item.Category] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTypeRent[item.Typology ?? item.Category]++;
                    }
                }

                result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth = propertiesInHome.ToList()
                    .GroupBy(obj => obj.CreationDate.Month + "/" + obj.CreationDate.Year.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                result.RealEstatePropertyHomeDetails.MaxAnnual = result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Any() ?
                    result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Max() : 0;

                result.RealEstatePropertyHomeDetails.MinAnnual = result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Any() ? 
                    result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Min() : 0;

                IQueryable<Request> request = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.Requests 
                    : _unitOfWork.dbContext.Requests.Where(x => x.UserId == agencyId);
                result.RequestHomeDetails.Total = request.Count();
                result.RequestHomeDetails.TotalActive = request.Where(x => !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalArchived = request.Where(x => x.Archived).Count();
                result.RequestHomeDetails.TotalClosed = request.Where(x => x.Closed).Count();
                result.RequestHomeDetails.TotalLastMonth = request.Where(x => x.CreationDate >= DateTime.UtcNow.AddMonths(-1) && !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalSale = request.Where(x => x.Contract == "Vendita" && !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalRent = request.Where(x => x.Contract == "Affitto" && !x.Closed && !x.Archived).Count();

                result.RequestHomeDetails.TotalCreatedPerMonth = request.ToList()
                    .GroupBy(obj => obj.CreationDate.Month + "/" + obj.CreationDate.Year.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                result.RequestHomeDetails.MaxAnnual = result.RequestHomeDetails.TotalCreatedPerMonth.Values.Count() > 0 ?
                    result.RequestHomeDetails.TotalCreatedPerMonth.Values.Max() : 0;

                result.RequestHomeDetails.MinAnnual = result.RequestHomeDetails.TotalCreatedPerMonth.Values.Count() > 0 ?
                    result.RequestHomeDetails.TotalCreatedPerMonth.Values.Min() : 0;

                foreach (var item in request.Where(x => x.Contract == "Vendita"))
                {
                    string[] cities = item.City.Split(',');
                    foreach(var city in cities)
                    {
                        if (!result.RequestHomeDetails.DistinctByCitySale.ContainsKey(city))
                        {
                            result.RequestHomeDetails.DistinctByCitySale[city] = 1;
                        }
                        else
                        {
                            result.RequestHomeDetails.DistinctByCitySale[city]++;
                        }
                    }

                    //By type
                    if (!result.RequestHomeDetails.DistinctByTypeSale.ContainsKey(item.PropertyType))
                    {
                        result.RequestHomeDetails.DistinctByTypeSale[item.PropertyType] = 1;
                    }
                    else
                    {
                        result.RequestHomeDetails.DistinctByTypeSale[item.PropertyType]++;
                    }
                }

                foreach (var item in request.Where(x => x.Contract == "Affitto"))
                {
                    string[] cities = item.City.Split(',');
                    foreach (var city in cities)
                    {
                        if (!result.RequestHomeDetails.DistinctByCityRent.ContainsKey(city))
                        {
                            result.RequestHomeDetails.DistinctByCityRent[city] = 1;
                        }
                        else
                        {
                            result.RequestHomeDetails.DistinctByCityRent[city]++;
                        }
                    }

                    //By type
                    if (!result.RequestHomeDetails.DistinctByTypeRent.ContainsKey(item.PropertyType))
                    {
                        result.RequestHomeDetails.DistinctByTypeRent[item.PropertyType] = 1;
                    }
                    else
                    {
                        result.RequestHomeDetails.DistinctByTypeRent[item.PropertyType]++;
                    }
                }

                result.TotalCustomers = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.Customers.Count() 
                    : _unitOfWork.dbContext.Customers.Where(x => x.UserId == agencyId).Count();
                result.TotalAgents = string.IsNullOrEmpty(agencyId) 
                    ? userManager.GetUsersInRoleAsync("Agent").Result.Count() 
                    : userManager.GetUsersInRoleAsync("Agent").Result.Where(x => x.AdminId == agencyId).Count();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }


        public async Task<DashboardAggregatedDataModel> GetDashboardAggregatedData(string? agencyId, int? year)
        {
            try
            {
                var result = new DashboardAggregatedDataModel();
                var now = DateTime.UtcNow;

                // Filtro base per agenzia
                IQueryable<RealEstateProperty> propertiesQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.RealEstateProperties 
                    : _unitOfWork.dbContext.RealEstateProperties.Where(x => x.User.AdminId == agencyId);

                IQueryable<Request> requestsQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.Requests 
                    : _unitOfWork.dbContext.Requests.Where(x => x.UserId == agencyId);

                IQueryable<Customer> customersQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.Customers 
                    : _unitOfWork.dbContext.Customers.Where(x => x.UserId == agencyId);

                IQueryable<Calendar> calendarQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.Calendars 
                    : _unitOfWork.dbContext.Calendars.Where(x => x.User.AdminId == agencyId);

                // Applica filtro anno se specificato
                if (year.HasValue)
                {
                    // Specifica DateTimeKind.Utc per PostgreSQL (richiede timestamp with timezone)
                    var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var endDate = new DateTime(year.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                    
                    propertiesQuery = propertiesQuery.Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate);
                    requestsQuery = requestsQuery.Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate);
                    customersQuery = customersQuery.Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate);
                    calendarQuery = calendarQuery.Where(x => x.EventStartDate >= startDate && x.EventStartDate <= endDate);
                }

                // ===== RealEstatePropertyHomeDetails =====
                // Ottimizzazione: usa CountAsync invece di caricare tutte le entità
                result.RealEstatePropertyHomeDetails.Total = await propertiesQuery.CountAsync();
                result.RealEstatePropertyHomeDetails.TotalSale = await propertiesQuery.Where(x => x.Status == "Vendita").CountAsync();
                result.RealEstatePropertyHomeDetails.TotalRent = await propertiesQuery.Where(x => x.Status == "Affitto").CountAsync();
                result.RealEstatePropertyHomeDetails.TotalLastMonth = await propertiesQuery.Where(x => x.CreationDate >= now.AddMonths(-1)).CountAsync();

                // Carica solo i campi necessari per le statistiche (City, Status, Typology, Category, CreationDate)
                var propertiesForStats = await propertiesQuery
                    .Select(x => new { x.City, x.Status, x.Typology, x.Category, x.CreationDate })
                    .ToListAsync();

                foreach (var item in propertiesForStats.Where(x => x.Status == "Vendita"))
                {
                    if (!result.RealEstatePropertyHomeDetails.DistinctByCitySale.ContainsKey(item.City))
                        result.RealEstatePropertyHomeDetails.DistinctByCitySale[item.City] = 1;
                    else
                        result.RealEstatePropertyHomeDetails.DistinctByCitySale[item.City]++;

                    var typeKey = item.Typology ?? item.Category;
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTypeSale.ContainsKey(typeKey))
                        result.RealEstatePropertyHomeDetails.DistinctByTypeSale[typeKey] = 1;
                    else
                        result.RealEstatePropertyHomeDetails.DistinctByTypeSale[typeKey]++;
                }

                foreach (var item in propertiesForStats.Where(x => x.Status == "Affitto"))
                {
                    if (!result.RealEstatePropertyHomeDetails.DistinctByCityRent.ContainsKey(item.City))
                        result.RealEstatePropertyHomeDetails.DistinctByCityRent[item.City] = 1;
                    else
                        result.RealEstatePropertyHomeDetails.DistinctByCityRent[item.City]++;

                    var typeKey = item.Typology ?? item.Category;
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTypeRent.ContainsKey(typeKey))
                        result.RealEstatePropertyHomeDetails.DistinctByTypeRent[typeKey] = 1;
                    else
                        result.RealEstatePropertyHomeDetails.DistinctByTypeRent[typeKey]++;
                }

                result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth = propertiesForStats
                    .GroupBy(obj => obj.CreationDate.Month + "/" + obj.CreationDate.Year.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                result.RealEstatePropertyHomeDetails.MaxAnnual = result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Any() ?
                    result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Max() : 0;

                result.RealEstatePropertyHomeDetails.MinAnnual = result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Any() ? 
                    result.RealEstatePropertyHomeDetails.TotalCreatedPerMonth.Values.Min() : 0;

                // ===== RequestHomeDetails =====
                var requestsList = await requestsQuery.ToListAsync();
                
                result.RequestHomeDetails.Total = requestsList.Count;
                result.RequestHomeDetails.TotalActive = requestsList.Where(x => !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalArchived = requestsList.Where(x => x.Archived).Count();
                result.RequestHomeDetails.TotalClosed = requestsList.Where(x => x.Closed).Count();
                result.RequestHomeDetails.TotalLastMonth = requestsList.Where(x => x.CreationDate >= now.AddMonths(-1) && !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalSale = requestsList.Where(x => x.Contract == "Vendita" && !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalRent = requestsList.Where(x => x.Contract == "Affitto" && !x.Closed && !x.Archived).Count();

                result.RequestHomeDetails.TotalCreatedPerMonth = requestsList
                    .GroupBy(obj => obj.CreationDate.Month + "/" + obj.CreationDate.Year.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                result.RequestHomeDetails.MaxAnnual = result.RequestHomeDetails.TotalCreatedPerMonth.Values.Any() ?
                    result.RequestHomeDetails.TotalCreatedPerMonth.Values.Max() : 0;

                result.RequestHomeDetails.MinAnnual = result.RequestHomeDetails.TotalCreatedPerMonth.Values.Any() ?
                    result.RequestHomeDetails.TotalCreatedPerMonth.Values.Min() : 0;

                foreach (var item in requestsList.Where(x => x.Contract == "Vendita"))
                {
                    string[] cities = item.City.Split(',');
                    foreach(var city in cities)
                    {
                        if (!result.RequestHomeDetails.DistinctByCitySale.ContainsKey(city))
                            result.RequestHomeDetails.DistinctByCitySale[city] = 1;
                        else
                            result.RequestHomeDetails.DistinctByCitySale[city]++;
                    }

                    if (!result.RequestHomeDetails.DistinctByTypeSale.ContainsKey(item.PropertyType))
                        result.RequestHomeDetails.DistinctByTypeSale[item.PropertyType] = 1;
                    else
                        result.RequestHomeDetails.DistinctByTypeSale[item.PropertyType]++;
                }

                foreach (var item in requestsList.Where(x => x.Contract == "Affitto"))
                {
                    string[] cities = item.City.Split(',');
                    foreach (var city in cities)
                    {
                        if (!result.RequestHomeDetails.DistinctByCityRent.ContainsKey(city))
                            result.RequestHomeDetails.DistinctByCityRent[city] = 1;
                        else
                            result.RequestHomeDetails.DistinctByCityRent[city]++;
                    }

                    if (!result.RequestHomeDetails.DistinctByTypeRent.ContainsKey(item.PropertyType))
                        result.RequestHomeDetails.DistinctByTypeRent[item.PropertyType] = 1;
                    else
                        result.RequestHomeDetails.DistinctByTypeRent[item.PropertyType]++;
                }

                // ===== Customers =====
                var customersList = await customersQuery.ToListAsync();
                result.TotalCustomers = customersList.Count;
                result.TotalBuyers = customersList.Where(x => x.Buyer).Count();
                result.TotalSellers = customersList.Where(x => x.Seller).Count();
                result.Customers = _mapper.Map<List<CustomerSelectModel>>(customersList);

                // ===== Calendar Events =====
                var calendarList = await calendarQuery.ToListAsync();
                result.TotalAppointments = calendarList.Where(x => !x.Cancelled).Count();
                result.TotalConfirmedAppointments = calendarList.Where(x => x.Confirmed && !x.Cancelled).Count();
                result.CalendarEvents = _mapper.Map<List<CalendarSelectModel>>(calendarList);

                // ===== Agents =====
                var agentsList = await userManager.GetUsersInRoleAsync("Agent");
                if (!string.IsNullOrEmpty(agencyId))
                {
                    agentsList = agentsList.Where(x => x.AdminId == agencyId).ToList();
                }
                result.TotalAgents = agentsList.Count;
                result.Agents = _mapper.Map<List<UserSelectModel>>(agentsList);

                // ===== Agencies (solo per superadmin) =====
                if (string.IsNullOrEmpty(agencyId))
                {
                    var agenciesList = await userManager.GetUsersInRoleAsync("Agency");
                    result.Agencies = _mapper.Map<List<UserSelectModel>>(agenciesList);
                }

                // ===== Properties Lists =====
                // Proprietà disponibili (non vendute e AssignmentEnd > oggi)
                // NOTA: Le proprietà disponibili NON devono essere filtrate per anno (vogliamo tutte quelle disponibili)
                // Ottimizzazione: usa proiezione diretta con Select() per caricare solo i campi necessari
                IQueryable<RealEstateProperty> availablePropertiesBaseQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.RealEstateProperties 
                    : _unitOfWork.dbContext.RealEstateProperties.Where(x => x.User.AdminId == agencyId);

                result.AvailableProperties = await availablePropertiesBaseQuery
                    .Where(p => !p.Sold && p.AssignmentEnd > now)
                    .Include(x => x.User)
                    .Include(x => x.Photos)
                    .Select(x => new RealEstatePropertyListModel
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = x.Price,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.Photos.OrderBy(p => p.Position).Select(p => p.Url).FirstOrDefault(),
                        AgencyId = x.User != null ? x.User.AdminId : null,
                        AgentId = x.UserId
                    })
                    .ToListAsync();

                // Proprietà vendute
                var soldPropertiesQuery = string.IsNullOrEmpty(agencyId) 
                    ? _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Sold) 
                    : _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Sold && x.User.AdminId == agencyId);

                if (year.HasValue)
                {
                    // Specifica DateTimeKind.Utc per PostgreSQL (richiede timestamp with timezone)
                    var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var endDate = new DateTime(year.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                    soldPropertiesQuery = soldPropertiesQuery.Where(x => 
                        (x.UpdateDate >= startDate && x.UpdateDate <= endDate) || 
                        (x.CreationDate >= startDate && x.CreationDate <= endDate));
                }

                // Ottimizzazione: usa proiezione diretta con Select() per caricare solo i campi necessari
                result.SoldProperties = await soldPropertiesQuery
                    .Include(x => x.User)
                    .Include(x => x.Photos)
                    .Select(x => new RealEstatePropertyListModel
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        AssignmentEnd = x.AssignmentEnd,
                        CommercialSurfaceate = x.CommercialSurfaceate,
                        AddressLine = x.AddressLine,
                        City = x.City,
                        State = x.State,
                        Price = x.Price,
                        Category = x.Category,
                        Typology = x.Typology,
                        StateOfTheProperty = x.StateOfTheProperty,
                        Status = x.Status,
                        Auction = x.Auction,
                        Sold = x.Sold,
                        FirstPhotoUrl = x.Photos.OrderBy(p => p.Position).Select(p => p.Url).FirstOrDefault(),
                        AgencyId = x.User != null ? x.User.AdminId : null,
                        AgentId = x.UserId
                    })
                    .ToListAsync();

                // ===== Requests =====
                result.Requests = _mapper.Map<List<RequestSelectModel>>(requestsList);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei dati aggregati della dashboard");
                throw new Exception("Si è verificato un errore nel recupero dei dati della dashboard");
            }
        }
    }
}
