using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.Repositories
{
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
        {
            return await _context.Set<SubscriptionPlan>()
                .Include(p => p.Features!.OrderBy(f => f.Id))
                .Where(p => p.Active)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetPlanWithFeaturesAsync(int id)
        {
            return await _context.Set<SubscriptionPlan>()
                .Include(p => p.Features!.OrderBy(f => f.Id))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsPlanNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _context.Set<SubscriptionPlan>().Where(p => p.Name == name);
            
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            
            return !await query.AnyAsync();
        }
    }
}
