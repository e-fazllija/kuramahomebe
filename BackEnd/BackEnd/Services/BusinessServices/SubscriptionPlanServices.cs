using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionPlanModels;

namespace BackEnd.Services.BusinessServices
{
    public class SubscriptionPlanServices : ISubscriptionPlanServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionPlanServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SubscriptionPlanSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<SubscriptionPlanSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<SubscriptionPlanSelectModel>> GetAllAsync()
        {
            var entities = await _unitOfWork.SubscriptionPlanRepository.GetListAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanSelectModel>>(entities);
        }

        public async Task<IEnumerable<SubscriptionPlanSelectModel>> GetActivePlansAsync()
        {
            var entities = await _unitOfWork.SubscriptionPlanRepository.GetActivePlansAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanSelectModel>>(entities);
        }

        public async Task<SubscriptionPlanSelectModel> CreateAsync(SubscriptionPlanCreateModel model)
        {
            var entity = _mapper.Map<SubscriptionPlan>(model);
            var createdEntity = await _unitOfWork.SubscriptionPlanRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<SubscriptionPlanSelectModel>(createdEntity);
        }

        public async Task<SubscriptionPlanSelectModel?> UpdateAsync(SubscriptionPlanUpdateModel model)
        {
            var existingEntity = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _unitOfWork.SubscriptionPlanRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<SubscriptionPlanSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.SubscriptionPlanRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Active = true;
            entity.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.SubscriptionPlanRepository.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Active = false;
            entity.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.SubscriptionPlanRepository.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.SubscriptionPlanRepository.IsPlanNameUniqueAsync(name, excludeId);
        }

        public async Task<SubscriptionPlanSelectModel?> GetPlanWithFeaturesAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionPlanRepository.GetPlanWithFeaturesAsync(id);
            return entity != null ? _mapper.Map<SubscriptionPlanSelectModel>(entity) : null;
        }
    }
}
