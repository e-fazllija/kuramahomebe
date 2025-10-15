using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Models.SubscriptionPlanModels;

namespace BackEnd.Services.BusinessServices
{
    public class SubscriptionPlanServices : ISubscriptionPlanServices
    {
        private readonly ISubscriptionPlanRepository _repository;
        private readonly IMapper _mapper;

        public SubscriptionPlanServices(ISubscriptionPlanRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SubscriptionPlanSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<SubscriptionPlanSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<SubscriptionPlanSelectModel>> GetAllAsync()
        {
            var entities = await _repository.GetListAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanSelectModel>>(entities);
        }

        public async Task<IEnumerable<SubscriptionPlanSelectModel>> GetActivePlansAsync()
        {
            var entities = await _repository.GetActivePlansAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanSelectModel>>(entities);
        }

        public async Task<SubscriptionPlanSelectModel> CreateAsync(SubscriptionPlanCreateModel model)
        {
            var entity = _mapper.Map<SubscriptionPlan>(model);
            var createdEntity = await _repository.CreateAsync(entity);
            return _mapper.Map<SubscriptionPlanSelectModel>(createdEntity);
        }

        public async Task<SubscriptionPlanSelectModel?> UpdateAsync(SubscriptionPlanUpdateModel model)
        {
            var existingEntity = await _repository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<SubscriptionPlanSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Active = true;
            entity.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Active = false;
            entity.UpdateDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return await _repository.IsPlanNameUniqueAsync(name, excludeId);
        }

        public async Task<SubscriptionPlanSelectModel?> GetPlanWithFeaturesAsync(int id)
        {
            var entity = await _repository.GetPlanWithFeaturesAsync(id);
            return entity != null ? _mapper.Map<SubscriptionPlanSelectModel>(entity) : null;
        }
    }
}
