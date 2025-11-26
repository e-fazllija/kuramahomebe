using BackEnd.Models.SubscriptionFeatureModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface ISubscriptionFeatureServices
    {
        Task<SubscriptionFeatureSelectModel?> GetByIdAsync(int id);
        Task<IEnumerable<SubscriptionFeatureSelectModel>> GetByPlanIdAsync(int planId);
        Task<SubscriptionFeatureSelectModel> CreateAsync(SubscriptionFeatureCreateModel model);
        Task<SubscriptionFeatureSelectModel?> UpdateAsync(SubscriptionFeatureUpdateModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByPlanIdAsync(int planId);
        Task<IEnumerable<SubscriptionFeatureSelectModel>> CreateMultipleAsync(IEnumerable<SubscriptionFeatureCreateModel> models);
    }
}
