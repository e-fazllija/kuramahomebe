using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Models.ProvinceModels;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.BusinessServices
{
    public class ProvinceServices
    {
        private readonly AppDbContext _context;

        public ProvinceServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Province> GetById(int id)
        {
            var province = await _context.Provinces
                .Include(p => p.Cities)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (province == null)
                throw new ArgumentException("Provincia non trovata");

            return province;
        }

        public async Task<List<ProvinceSelectModel>> Get(string? filterRequest = null)
        {
            var query = _context.Provinces.AsQueryable();

            if (!string.IsNullOrEmpty(filterRequest))
            {
                query = query.Where(p => p.Name.Contains(filterRequest));
            }

            var provinces = await query
                .OrderBy(p => p.Name)
                .Select(p => new ProvinceSelectModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return provinces;
        }

        public async Task<List<ProvinceSelectModel>> GetAll()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .Select(p => new ProvinceSelectModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return provinces;
        }

    }
} 