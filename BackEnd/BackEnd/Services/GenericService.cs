using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Models.Options;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RealEstatePropertyPhotoModels;
using BackEnd.Models.LocationModels;
using BackEnd.Services.BusinessServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities;
using System;
using BackEnd.Interfaces.IBusinessServices;

namespace BackEnd.Services
{
    public class GenericService : IGenericService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GenericService> _logger;
        private readonly IOptionsMonitor<PaginationOptions> options;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILocationServices _locationServices;
        
        public GenericService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GenericService> logger, IOptionsMonitor<PaginationOptions> options, UserManager<ApplicationUser> userManager, ILocationServices locationServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            this.options = options;
            this.userManager = userManager;
            _locationServices = locationServices;
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
                    : _unitOfWork.dbContext.RealEstateProperties.Where(x => x.Agent.AgencyId == agencyId);

                result.RealEstatePropertyHomeDetails.Total = propertiesInHome.Count();
                result.RealEstatePropertyHomeDetails.TotalSale = propertiesInHome.Where(x => x.Status == "Vendita").Count();
                result.RealEstatePropertyHomeDetails.TotalRent = propertiesInHome.Where(x => x.Status == "Affitto").Count();
                result.RealEstatePropertyHomeDetails.TotalLastMonth = propertiesInHome.Where(x => x.CreationDate >= DateTime.Now.AddMonths(-1)).Count();

                foreach (var item in propertiesInHome.Where(x => x.Status == "Vendita"))
                {
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTownSale.ContainsKey(item.Town))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTownSale[item.Town] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTownSale[item.Town]++;
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
                    if (!result.RealEstatePropertyHomeDetails.DistinctByTownRent.ContainsKey(item.Town))
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTownRent[item.Town] = 1;
                    }
                    else
                    {
                        result.RealEstatePropertyHomeDetails.DistinctByTownRent[item.Town]++;
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
                    : _unitOfWork.dbContext.Requests.Where(x => x.AgencyId == agencyId);
                result.RequestHomeDetails.Total = request.Count();
                result.RequestHomeDetails.TotalActive = request.Where(x => !x.Closed && !x.Archived).Count();
                result.RequestHomeDetails.TotalArchived = request.Where(x => x.Archived).Count();
                result.RequestHomeDetails.TotalClosed = request.Where(x => x.Closed).Count();
                result.RequestHomeDetails.TotalLastMonth = request.Where(x => x.CreationDate >= DateTime.Now.AddMonths(-1) && !x.Closed && !x.Archived).Count();
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
                    string[] towns = item.Town.Split(',');
                    foreach(var town in towns)
                    {
                        if (!result.RequestHomeDetails.DistinctByTownSale.ContainsKey(town))
                        {
                            result.RequestHomeDetails.DistinctByTownSale[town] = 1;
                        }
                        else
                        {
                            result.RequestHomeDetails.DistinctByTownSale[town]++;
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
                    string[] towns = item.Town.Split(',');
                    foreach (var town in towns)
                    {
                        if (!result.RequestHomeDetails.DistinctByTownRent.ContainsKey(town))
                        {
                            result.RequestHomeDetails.DistinctByTownRent[town] = 1;
                        }
                        else
                        {
                            result.RequestHomeDetails.DistinctByTownRent[town]++;
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
                    : _unitOfWork.dbContext.Customers.Where(x => x.AgencyId == agencyId).Count();
                result.TotalAgents = string.IsNullOrEmpty(agencyId) 
                    ? userManager.GetUsersInRoleAsync("Agent").Result.Count() 
                    : userManager.GetUsersInRoleAsync("Agent").Result.Where(x => x.AgencyId == agencyId).Count();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore");
            }
        }

        public async Task<List<LocationSelectModel>> GetLocations()
        {
            try
            {
                return await _locationServices.GetAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Si è verificato un errore nel recupero delle località");
            }
        }
    }
}
