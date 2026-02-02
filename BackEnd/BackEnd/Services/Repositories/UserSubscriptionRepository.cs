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

        public override async Task<UserSubscription?> GetByIdAsync(int id)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                .Include(us => us.LastPayment)
                .FirstOrDefaultAsync(us => us.Id == id);
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

        public async Task<UserSubscription?> GetActiveUserSubscriptionAsync(string userId, string? agencyId = null)
        {
            // 1. Cerca prima l'abbonamento diretto dell'utente
            var directSubscription = await _context.Set<UserSubscription>()
                .Include(us => us.SubscriptionPlan)
                    .ThenInclude(sp => sp.Features)
                .Include(us => us.LastPayment)
                .Where(us => us.UserId == userId && us.Status.ToLower() == "active")
                .OrderByDescending(us => us.CreationDate)
                .FirstOrDefaultAsync();

            // Se ha un abbonamento diretto, lo restituisce
            if (directSubscription != null)
                return directSubscription;

            // 2. Se non ha abbonamento diretto ma ha AgencyId, cerca l'abbonamento dell'agenzia parent (prima risalita)
            if (!string.IsNullOrEmpty(agencyId))
            {
                var agencySubscription = await _context.Set<UserSubscription>()
                    .Include(us => us.SubscriptionPlan)
                        .ThenInclude(sp => sp.Features)
                    .Include(us => us.LastPayment)
                    .Where(us => us.UserId == agencyId && us.Status.ToLower() == "active")
                    .OrderByDescending(us => us.CreationDate)
                    .FirstOrDefaultAsync();

                // Se l'Agency ha un abbonamento, lo restituisce
                if (agencySubscription != null)
                    return agencySubscription;

                // 3. Se l'Agency non ha abbonamento, verifica se ha un proprio AgencyId (seconda risalita fino all'Admin)
                var agencyUser = await _context.Set<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == agencyId);

                if (agencyUser != null && !string.IsNullOrEmpty(agencyUser.AdminId))
                {
                    // Risali fino all'Admin cercando l'abbonamento dell'Admin
                    var adminSubscription = await _context.Set<UserSubscription>()
                        .Include(us => us.SubscriptionPlan)
                            .ThenInclude(sp => sp.Features)
                        .Include(us => us.LastPayment)
                        .Where(us => us.UserId == agencyUser.AdminId && us.Status.ToLower() == "active")
                        .OrderByDescending(us => us.CreationDate)
                        .FirstOrDefaultAsync();

                    return adminSubscription;
                }
            }

            return null;
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
