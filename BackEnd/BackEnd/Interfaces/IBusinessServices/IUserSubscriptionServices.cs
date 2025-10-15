using BackEnd.Models.UserSubscriptionModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IUserSubscriptionServices
    {
        Task<UserSubscriptionSelectModel?> GetByIdAsync(int id);
        Task<IEnumerable<UserSubscriptionSelectModel>> GetUserSubscriptionsAsync(string userId);
        Task<UserSubscriptionSelectModel?> GetActiveUserSubscriptionAsync(string userId);
        Task<UserSubscriptionSelectModel> CreateAsync(UserSubscriptionCreateModel model);
        Task<UserSubscriptionSelectModel?> UpdateAsync(UserSubscriptionUpdateModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelSubscriptionAsync(int id);
        Task<bool> RenewSubscriptionAsync(int id);
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task<UserSubscriptionSelectModel?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<IEnumerable<UserSubscriptionSelectModel>> GetExpiredSubscriptionsAsync();
        Task<bool> CheckSubscriptionLimitsAsync(string userId, int planId);
    }
}
