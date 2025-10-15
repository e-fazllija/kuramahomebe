using BackEnd.Models.PaymentModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IPaymentServices
    {
        Task<PaymentSelectModel?> GetByIdAsync(int id);
        Task<IEnumerable<PaymentSelectModel>> GetUserPaymentsAsync(string userId);
        Task<IEnumerable<PaymentSelectModel>> GetPaymentsBySubscriptionIdAsync(int subscriptionId);
        Task<PaymentSelectModel> CreateAsync(PaymentCreateModel model);
        Task<PaymentSelectModel?> UpdateAsync(PaymentUpdateModel model);
        Task<bool> DeleteAsync(int id);
        Task<PaymentSelectModel?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);
        Task<PaymentSelectModel?> GetByTransactionIdAsync(string transactionId);
        Task<bool> UpdatePaymentStatusAsync(int id, string status);
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<PaymentSelectModel>> GetPaymentsByStatusAsync(string status);
        Task<bool> ProcessStripePaymentAsync(string paymentIntentId, string status);
    }
}
