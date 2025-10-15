using BackEnd.Entities;

namespace BackEnd.Interfaces.IRepositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetUserPaymentsAsync(string userId);
        Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetPaymentsBySubscriptionIdAsync(int subscriptionId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
    }
}
