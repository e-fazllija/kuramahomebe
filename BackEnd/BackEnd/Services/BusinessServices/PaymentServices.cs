using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.PaymentModels;

namespace BackEnd.Services.BusinessServices
{
    public class PaymentServices : IPaymentServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<PaymentSelectModel>> GetUserPaymentsAsync(string userId)
        {
            var entities = await _unitOfWork.PaymentRepository.GetUserPaymentsAsync(userId);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<IEnumerable<PaymentSelectModel>> GetPaymentsBySubscriptionIdAsync(int subscriptionId)
        {
            var entities = await _unitOfWork.PaymentRepository.GetPaymentsBySubscriptionIdAsync(subscriptionId);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<PaymentSelectModel> CreateAsync(PaymentCreateModel model)
        {
            var entity = _mapper.Map<Payment>(model);
            var createdEntity = await _unitOfWork.PaymentRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<PaymentSelectModel>(createdEntity);
        }

        public async Task<PaymentSelectModel?> UpdateAsync(PaymentUpdateModel model)
        {
            var existingEntity = await _unitOfWork.PaymentRepository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _unitOfWork.PaymentRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<PaymentSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.PaymentRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<PaymentSelectModel?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId)
        {
            var entity = await _unitOfWork.PaymentRepository.GetByStripePaymentIntentIdAsync(stripePaymentIntentId);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<PaymentSelectModel?> GetByTransactionIdAsync(string transactionId)
        {
            var entity = await _unitOfWork.PaymentRepository.GetByTransactionIdAsync(transactionId);
            return entity != null ? _mapper.Map<PaymentSelectModel>(entity) : null;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string status)
        {
            var entity = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = status;
            entity.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.PaymentRepository.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var payments = await _unitOfWork.PaymentRepository.GetPaymentsByStatusAsync("completed");
            
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
            var entities = await _unitOfWork.PaymentRepository.GetPaymentsByStatusAsync(status);
            return _mapper.Map<IEnumerable<PaymentSelectModel>>(entities);
        }

        public async Task<bool> ProcessStripePaymentAsync(string paymentIntentId, string status)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByStripePaymentIntentIdAsync(paymentIntentId);
            if (payment == null) return false;

            payment.Status = status;
            payment.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}