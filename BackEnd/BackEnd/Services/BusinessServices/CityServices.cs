using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Models.CityModels;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.BusinessServices
{
    public class CityServices
    {
        private readonly AppDbContext _context;

        public CityServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<City> GetById(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Province)
                .Include(c => c.Locations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
                throw new ArgumentException("Città non trovata");

            return city;
        }

        public async Task<List<CitySelectModel>> Get(string? filterRequest = null, int? provinceId = null)
        {
            var query = _context.Cities
                .Include(c => c.Province)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterRequest))
            {
                query = query.Where(c => c.Name.Contains(filterRequest));
            }

            if (provinceId.HasValue)
            {
                query = query.Where(c => c.ProvinceId == provinceId.Value);
            }

            var cities = await query
                .OrderBy(c => c.Name)
                .Select(c => new CitySelectModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProvinceId = c.ProvinceId,
                    ProvinceName = c.Province.Name
                })
                .ToListAsync();

            return cities;
        }

        public async Task<List<CitySelectModel>> GetAll()
        {
            var cities = await _context.Cities
                .Include(c => c.Province)
                .OrderBy(c => c.Name)
                .Select(c => new CitySelectModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProvinceId = c.ProvinceId,
                    ProvinceName = c.Province.Name
                })
                .ToListAsync();

            return cities;
        }

        public async Task<List<CitySelectModel>> GetByProvince(int provinceId)
        {
            var cities = await _context.Cities
                .Include(c => c.Province)
                .Where(c => c.ProvinceId == provinceId)
                .OrderBy(c => c.Name)
                .Select(c => new CitySelectModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProvinceId = c.ProvinceId,
                    ProvinceName = c.Province.Name
                })
                .ToListAsync();

            return cities;
        }

        public async Task<List<CityListModel>> GetList(string? filterRequest = null, int? provinceId = null)
        {
            var query = _context.Cities
                .Include(c => c.Province)
                .Include(c => c.Locations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterRequest))
            {
                query = query.Where(c => c.Name.Contains(filterRequest) || c.Province.Name.Contains(filterRequest));
            }

            if (provinceId.HasValue)
            {
                query = query.Where(c => c.ProvinceId == provinceId.Value);
            }

            var cities = await query
                .OrderBy(c => c.Province.Name)
                .ThenBy(c => c.Name)
                .Select(c => new CityListModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProvinceName = c.Province.Name,
                    LocationsCount = c.Locations.Count
                })
                .ToListAsync();

            return cities;
        }

        public async Task<List<CityGroupedModel>> GetGroupedByProvince()
        {
            var cities = await _context.Cities
                .Include(c => c.Province)
                .OrderBy(c => c.Province.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var grouped = cities
                .GroupBy(c => c.Province.Name)
                .Select(g => new CityGroupedModel
                {
                    Province = g.Key,
                    Cities = g.Select(c => new CityItemModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList()
                })
                .ToList();

            return grouped;
        }

        public async Task<bool> Exists(string name, int provinceId, int? excludeId = null)
        {
            var query = _context.Cities
                .Where(c => c.Name.ToLower() == name.ToLower() && c.ProvinceId == provinceId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}