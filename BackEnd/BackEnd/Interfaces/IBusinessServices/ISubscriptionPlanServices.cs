using BackEnd.Models.SubscriptionPlanModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface ISubscriptionPlanServices
    {
        Task<SubscriptionPlanSelectModel?> GetByIdAsync(int id);
        Task<IEnumerable<SubscriptionPlanSelectModel>> GetAllAsync();
        Task<IEnumerable<SubscriptionPlanSelectModel>> GetActivePlansAsync();
        Task<SubscriptionPlanSelectModel> CreateAsync(SubscriptionPlanCreateModel model);
        Task<SubscriptionPlanSelectModel?> UpdateAsync(SubscriptionPlanUpdateModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<SubscriptionPlanSelectModel?> GetPlanWithFeaturesAsync(int id);
    }
}
