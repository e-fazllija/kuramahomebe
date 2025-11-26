using BackEnd.Entities;

namespace BackEnd.Interfaces.IRepositories
{
    public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
    {
        Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(string userId);
        Task<UserSubscription?> GetActiveUserSubscriptionAsync(string userId, string? agencyId = null);
        Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<IEnumerable<UserSubscription>> GetExpiredSubscriptionsAsync();
        Task<IEnumerable<UserSubscription>> GetSubscriptionsByPlanIdAsync(int planId);
        Task<bool> HasActiveSubscriptionAsync(string userId);
    }
}
