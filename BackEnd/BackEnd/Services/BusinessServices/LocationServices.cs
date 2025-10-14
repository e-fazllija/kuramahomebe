using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.LocationModels;
using BackEnd.Models.OutputModels;
using BackEnd.Services;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.BusinessServices
{
    public class LocationServices : ILocationServices
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LocationServices> _logger;

        public LocationServices(AppDbContext context, ILogger<LocationServices> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LocationSelectModel> Create(LocationCreateModel dto)
        {
            try
            {
                // Verifica che la città esista e includi la provincia
                var city = await _context.Cities
                    .Include(c => c.Province)
                    .FirstOrDefaultAsync(c => c.Id == dto.CityId);
                if (city == null)
                    throw new ArgumentException("Città non trovata");

                var location = new Location
                {
                    Name = dto.Name,
                    CityId = dto.CityId,
                    CreationDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                return new LocationSelectModel
                {
                    Id = location.Id,
                    Name = location.Name,
                    CityId = location.CityId,
                    CityName = city.Name,
                    ProvinceId = city.ProvinceId,
                    ProvinceName = city.Province.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                throw;
            }
        }

        public async Task<LocationSelectModel> Update(LocationUpdateModel dto)
        {
            try
            {
                var location = await _context.Locations.FindAsync(dto.Id);
                if (location == null)
                    throw new Exception("Location not found");

                // Verifica che la città esista e includi la provincia
                var city = await _context.Cities
                    .Include(c => c.Province)
                    .FirstOrDefaultAsync(c => c.Id == dto.CityId);
                if (city == null)
                    throw new ArgumentException("Città non trovata");

                location.Name = dto.Name;
                location.CityId = dto.CityId;
                location.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return new LocationSelectModel
                {
                    Id = location.Id,
                    Name = location.Name,
                    CityId = location.CityId,
                    CityName = city.Name,
                    ProvinceId = city.ProvinceId,
                    ProvinceName = city.Province.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                throw;
            }
        }

        public async Task<LocationSelectModel> GetById(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.City)
                    .ThenInclude(c => c.Province)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                    throw new Exception("Location not found");

                return new LocationSelectModel
                {
                    Id = location.Id,
                    Name = location.Name,
                    CityId = location.CityId,
                    CityName = location.City.Name,
                    ProvinceId = location.City.ProvinceId,
                    ProvinceName = location.City.Province.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location by id");
                throw;
            }
        }

        public async Task<ListViewModel<LocationSelectModel>> Get(int currentPage, string? filterRequest, string? city)
        {
            try
            {
                var query = _context.Locations
                    .Include(l => l.City)
                    .ThenInclude(c => c.Province)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filterRequest))
                {
                    query = query.Where(l => l.Name.Contains(filterRequest) || l.City.Name.Contains(filterRequest) || l.City.Province.Name.Contains(filterRequest));
                }

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(l => l.City.Name == city);
                }

                query = query.OrderBy(l => l.City.Name).ThenBy(l => l.Name);

                var totalCount = await query.CountAsync();
                var pageSize = 20;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var locations = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new LocationSelectModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CityId = l.CityId,
                        CityName = l.City.Name,
                        ProvinceId = l.City.ProvinceId,
                        ProvinceName = l.City.Province.Name
                    })
                    .ToListAsync();

                return new ListViewModel<LocationSelectModel>
                {
                    Data = locations,
                    Total = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations");
                throw;
            }
        }

        public async Task<List<LocationSelectModel>> GetAll()
        {
            try
            {
                return await _context.Locations
                    .Include(l => l.City)
                    .ThenInclude(c => c.Province)
                    .OrderBy(l => l.City.Name)
                    .ThenBy(l => l.Name)
                    .Select(l => new LocationSelectModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CityId = l.CityId,
                        CityName = l.City.Name,
                        ProvinceId = l.City.ProvinceId,
                        ProvinceName = l.City.Province.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all locations");
                throw;
            }
        }

        public async Task<List<LocationGroupedModel>> GetGroupedByCity()
        {
            try
            {
                var locations = await _context.Locations
                    .Include(l => l.City)
                    .OrderBy(l => l.City.Name)
                    .ThenBy(l => l.Name)
                    .ToListAsync();

                var grouped = locations
                    .GroupBy(l => l.City.Name)
                    .Select(g => new LocationGroupedModel
                    {
                        City = g.Key,
                        Locations = g.Select(l => new LocationItemModel
                        {
                            Id = l.Name,
                            Name = l.Name
                        }).ToList()
                    })
                    .ToList();

                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped locations");
                throw;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                    throw new Exception("Location not found");

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location");
                throw;
            }
        }

        public async Task ToggleActive(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                    throw new Exception("Location not found");

                location.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling location active status");
                throw;
            }
        }

        public async Task UpdateOrder(List<LocationUpdateModel> locations)
        {
            try
            {
                foreach (var locationDto in locations)
                {
                    var location = await _context.Locations.FindAsync(locationDto.Id);
                    if (location != null)
                    {
                        location.UpdateDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location order");
                throw;
            }
        }

        public async Task<bool> SeedLocations()
        {
            try
            {
                await LocationDataSeeder.SeedLocations(_context);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding locations");
                throw;
            }
        }
    }
} 