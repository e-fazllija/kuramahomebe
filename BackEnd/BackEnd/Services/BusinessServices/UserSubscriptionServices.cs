using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Models.UserSubscriptionModels;

namespace BackEnd.Services.BusinessServices
{
    public class UserSubscriptionServices : IUserSubscriptionServices
    {
        private readonly IUserSubscriptionRepository _repository;
        private readonly IMapper _mapper;

        public UserSubscriptionServices(IUserSubscriptionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserSubscriptionSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<UserSubscriptionSelectModel>> GetUserSubscriptionsAsync(string userId)
        {
            var entities = await _repository.GetUserSubscriptionsAsync(userId);
            return _mapper.Map<IEnumerable<UserSubscriptionSelectModel>>(entities);
        }

        public async Task<UserSubscriptionSelectModel?> GetActiveUserSubscriptionAsync(string userId)
        {
            var entity = await _repository.GetActiveUserSubscriptionAsync(userId);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<UserSubscriptionSelectModel> CreateAsync(UserSubscriptionCreateModel model)
        {
            var entity = _mapper.Map<UserSubscription>(model);
            var createdEntity = await _repository.CreateAsync(entity);
            return _mapper.Map<UserSubscriptionSelectModel>(createdEntity);
        }

        public async Task<UserSubscriptionSelectModel?> UpdateAsync(UserSubscriptionUpdateModel model)
        {
            var existingEntity = await _repository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<UserSubscriptionSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> CancelSubscriptionAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "cancelled";
            entity.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "active";
            entity.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            return await _repository.HasActiveSubscriptionAsync(userId);
        }

        public async Task<UserSubscriptionSelectModel?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            var entity = await _repository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            return entity != null ? _mapper.Map<UserSubscriptionSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<UserSubscriptionSelectModel>> GetExpiredSubscriptionsAsync()
        {
            var entities = await _repository.GetExpiredSubscriptionsAsync();
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