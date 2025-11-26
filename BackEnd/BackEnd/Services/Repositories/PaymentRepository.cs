using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetUserPaymentsAsync(string userId)
        {
            return await _context.Set<Payment>()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId)
        {
            return await _context.Set<Payment>()
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Set<Payment>()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsBySubscriptionIdAsync(int subscriptionId)
        {
            return await _context.Set<Payment>()
                .Where(p => p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _context.Set<Payment>()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
