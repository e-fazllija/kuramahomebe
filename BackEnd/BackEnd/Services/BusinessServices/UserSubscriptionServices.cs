using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.UserSubscriptionModels;

namespace BackEnd.Services.BusinessServices
{
    public class UserSubscriptionServices : IUserSubscriptionServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserSubscriptionServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserSubscriptionSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<UserSubscriptionSelectModel>> GetUserSubscriptionsAsync(string userId)
        {
            var entities = await _unitOfWork.UserSubscriptionRepository.GetUserSubscriptionsAsync(userId);
            return _mapper.Map<IEnumerable<UserSubscriptionSelectModel>>(entities);
        }

        public async Task<UserSubscriptionSelectModel?> GetActiveUserSubscriptionAsync(string userId, string? agencyId = null)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetActiveUserSubscriptionAsync(userId, agencyId);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<UserSubscriptionSelectModel> CreateAsync(UserSubscriptionCreateModel model)
        {
            var entity = _mapper.Map<UserSubscription>(model);
            var createdEntity = await _unitOfWork.UserSubscriptionRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<UserSubscriptionSelectModel>(createdEntity);
        }

        public async Task<UserSubscriptionSelectModel?> UpdateAsync(UserSubscriptionUpdateModel model)
        {
            var existingEntity = await _unitOfWork.UserSubscriptionRepository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _unitOfWork.UserSubscriptionRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<UserSubscriptionSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.UserSubscriptionRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int id)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "cancelled";
            entity.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.UserSubscriptionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int id)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "active";
            entity.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.UserSubscriptionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            return await _unitOfWork.UserSubscriptionRepository.HasActiveSubscriptionAsync(userId);
        }

        public async Task<bool> HasPremiumPlanAsync(string userId)
        {
            var activeSubscription = await _unitOfWork.UserSubscriptionRepository.GetActiveUserSubscriptionAsync(userId, null);
            if (activeSubscription == null) return false;
            
            var planName = activeSubscription.SubscriptionPlan?.Name?.ToLowerInvariant() ?? "";
            var status = activeSubscription.Status?.ToLowerInvariant() ?? "";
            
            return planName == "premium" && status == "active";
        }

        public async Task<UserSubscriptionSelectModel?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<UserSubscriptionSelectModel>> GetExpiredSubscriptionsAsync()
        {
            var entities = await _unitOfWork.UserSubscriptionRepository.GetExpiredSubscriptionsAsync();
            return _mapper.Map<IEnumerable<UserSubscriptionSelectModel>>(entities);
        }

        public async Task<bool> CheckSubscriptionLimitsAsync(string userId, int planId)
        {
            // Implementare logica per verificare i limiti dell'abbonamento
            // Questo è un placeholder - implementare la logica specifica
            return true;
        }
    }
}