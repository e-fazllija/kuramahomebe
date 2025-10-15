using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.Repositories
{
    public class SubscriptionFeatureRepository : GenericRepository<SubscriptionFeature>, ISubscriptionFeatureRepository
    {
        public SubscriptionFeatureRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SubscriptionFeature>> GetFeaturesByPlanIdAsync(int planId)
        {
            return await _context.Set<SubscriptionFeature>()
                .Where(f => f.SubscriptionPlanId == planId)
                .OrderBy(f => f.FeatureName)
                .ToListAsync();
        }

        public async Task<bool> DeleteFeaturesByPlanIdAsync(int planId)
        {
            var features = await _context.Set<SubscriptionFeature>()
                .Where(f => f.SubscriptionPlanId == planId)
                .ToListAsync();

            if (features.Any())
            {
                _context.Set<SubscriptionFeature>().RemoveRange(features);
                return true;
            }

            return false;
        }
    }
}
