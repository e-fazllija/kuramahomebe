using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Models.PaymentModels;

namespace BackEnd.Services.BusinessServices
{
    public class PaymentServices : IPaymentServices
    {
        private readonly IPaymentRepository _repository;
        private readonly IMapper _mapper;

        public PaymentServices(IPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaymentSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<PaymentSelectModel>> GetUserPaymentsAsync(string userId)
        {
            var entities = await _repository.GetUserPaymentsAsync(userId);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<IEnumerable<PaymentSelectModel>> GetPaymentsBySubscriptionIdAsync(int subscriptionId)
        {
            var entities = await _repository.GetPaymentsBySubscriptionIdAsync(subscriptionId);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<PaymentSelectModel> CreateAsync(PaymentCreateModel model)
        {
            var entity = _mapper.Map<Payment>(model);
            var createdEntity = await _repository.CreateAsync(entity);
            return _mapper.Map<PaymentSelectModel>(createdEntity);
        }

        public async Task<PaymentSelectModel?> UpdateAsync(PaymentUpdateModel model)
        {
            var existingEntity = await _repository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<PaymentSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<PaymentSelectModel?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId)
        {
            var entity = await _repository.GetByStripePaymentIntentIdAsync(stripePaymentIntentId);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<PaymentSelectModel?> GetByTransactionIdAsync(string transactionId)
        {
            var entity = await _repository.GetByTransactionIdAsync(transactionId);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string status)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = status;
            entity.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var payments = await _repository.GetPaymentsByStatusAsync("completed");
            
            if (fromDate.HasValue || toDate.HasValue)
            {
                payments = payments.Where(p => 
                    (!fromDate.HasValue || p.PaymentDate >= fromDate.Value) &&
                    (!toDate.HasValue || p.PaymentDate <= toDate.Value)
                );
            }
            
            return payments.Sum(p => p.Amount);
        }

        public async Task<IEnumerable<PaymentSelectModel>> GetPaymentsByStatusAsync(string status)
        {
            var entities = await _repository.GetPaymentsByStatusAsync(status);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<bool> ProcessStripePaymentAsync(string paymentIntentId, string status)
        {
            var payment = await _repository.GetByStripePaymentIntentIdAsync(paymentIntentId);
            if (payment == null) return false;

            payment.Status = status;
            payment.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(payment);
            return true;
        }
    }
}