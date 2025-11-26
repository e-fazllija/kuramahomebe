using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.Repositories
{
    public class StripeWebhookEventRepository : GenericRepository<StripeWebhookEvent>, IStripeWebhookEventRepository
    {
        public StripeWebhookEventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<StripeWebhookEvent?> GetByEventIdAsync(string eventId)
        {
            return await _context.Set<StripeWebhookEvent>()
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        public async Task<IEnumerable<StripeWebhookEvent>> GetUnprocessedEventsAsync()
        {
            return await _context.Set<StripeWebhookEvent>()
                .Where(e => !e.Processed)
                .OrderBy(e => e.ReceivedAt)
                .ToListAsync();
        }

        public async Task<bool> IsEventProcessedAsync(string eventId)
        {
            return await _context.Set<StripeWebhookEvent>()
                .AnyAsync(e => e.EventId == eventId && e.Processed);
        }

        public async Task<bool> MarkAsProcessedAsync(int id)
        {
            var entity = await _context.Set<StripeWebhookEvent>().FindAsync(id);
            if (entity == null) return false;

            entity.Processed = true;
            entity.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
