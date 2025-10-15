using BackEnd.Models.StripeWebhookEventModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IStripeWebhookEventServices
    {
        Task<StripeWebhookEventSelectModel?> GetByIdAsync(int id);
        Task<IEnumerable<StripeWebhookEventSelectModel>> GetAllAsync();
        Task<StripeWebhookEventSelectModel> CreateAsync(StripeWebhookEventCreateModel model);
        Task<StripeWebhookEventSelectModel?> UpdateAsync(StripeWebhookEventUpdateModel model);
        Task<bool> DeleteAsync(int id);
        Task<StripeWebhookEventSelectModel?> GetByEventIdAsync(string eventId);
        Task<IEnumerable<StripeWebhookEventSelectModel>> GetUnprocessedEventsAsync();
        Task<bool> IsEventProcessedAsync(string eventId);
        Task<bool> MarkAsProcessedAsync(int id);
        Task<bool> ProcessWebhookEventAsync(string eventId, string eventType, string data);
    }
}
