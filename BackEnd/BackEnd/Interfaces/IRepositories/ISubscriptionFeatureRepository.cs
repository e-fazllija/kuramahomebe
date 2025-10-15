using BackEnd.Entities;

namespace BackEnd.Interfaces.IRepositories
{
    public interface ISubscriptionFeatureRepository : IGenericRepository<SubscriptionFeature>
    {
        Task<IEnumerable<SubscriptionFeature>> GetFeaturesByPlanIdAsync(int planId);
        Task<bool> DeleteFeaturesByPlanIdAsync(int planId);
    }
}
