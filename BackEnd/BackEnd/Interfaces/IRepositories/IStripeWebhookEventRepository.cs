using BackEnd.Entities;

namespace BackEnd.Interfaces.IRepositories
{
    public interface IStripeWebhookEventRepository : IGenericRepository<StripeWebhookEvent>
    {
        Task<StripeWebhookEvent?> GetByEventIdAsync(string eventId);
        Task<IEnumerable<StripeWebhookEvent>> GetUnprocessedEventsAsync();
        Task<bool> IsEventProcessedAsync(string eventId);
        Task<bool> MarkAsProcessedAsync(int id);
    }
}
