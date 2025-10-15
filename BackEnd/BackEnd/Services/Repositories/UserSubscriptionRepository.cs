using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.Repositories
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(string userId)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                .Include(us => us.LastPayment)
                .Where(us => us.UserId == userId)
                .OrderByDescending(us => us.CreationDate)
                .ToListAsync();
        }

        public async Task<UserSubscription?> GetActiveUserSubscriptionAsync(string userId)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                .Include(us => us.LastPayment)
                .Where(us => us.UserId == userId && us.Status == "active")
                .OrderByDescending(us => us.CreationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                .Include(us => us.LastPayment)
                .FirstOrDefaultAsync(us => us.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task<IEnumerable<UserSubscription>> GetExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                .Where(us => us.Status == "active" && us.EndDate.HasValue && us.EndDate.Value < now)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSubscription>> GetSubscriptionsByPlanIdAsync(int planId)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.User)
                .Where(us => us.SubscriptionPlanId == planId)
                .OrderByDescending(us => us.CreationDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            return await _context.Set<UserSubscription>()
                .AnyAsync(us => us.UserId == userId && us.Status == "active");
        }
    }
}
