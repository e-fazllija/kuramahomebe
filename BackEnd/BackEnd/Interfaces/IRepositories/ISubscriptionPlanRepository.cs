using BackEnd.Entities;

namespace BackEnd.Interfaces.IRepositories
{
    public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
    {
        Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync();
        Task<SubscriptionPlan?> GetPlanWithFeaturesAsync(int id);
        Task<bool> IsPlanNameUniqueAsync(string name, int? excludeId = null);
    }
}
