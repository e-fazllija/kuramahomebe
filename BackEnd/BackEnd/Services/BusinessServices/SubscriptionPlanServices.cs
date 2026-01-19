using System;
using System.Linq;
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
            var plans = _mapper.Map<IEnumerable<SubscriptionPlanSelectModel>>(entities).ToList();
            
            // Se il piano Free non ha features, eredita quelle del Basic
            var freePlan = plans.FirstOrDefault(p => p.Name.Equals("Free", StringComparison.OrdinalIgnoreCase));
            if (freePlan != null && (freePlan.Features == null || !freePlan.Features.Any()))
            {
                var basicPlan = plans.FirstOrDefault(p => p.Name.Equals("Basic", StringComparison.OrdinalIgnoreCase));
                if (basicPlan != null && basicPlan.Features != null && basicPlan.Features.Any())
                {
                    // Copia le features del Basic al Free, ma mantieni il SubscriptionPlanId del Free
                    freePlan.Features = basicPlan.Features.Select(f => new Models.SubscriptionFeatureModels.SubscriptionFeatureSelectModel
                    {
                        Id = f.Id,
                        SubscriptionPlanId = freePlan.Id, // Usa l'ID del Free
                        FeatureName = f.FeatureName,
                        FeatureValue = f.FeatureValue,
                        Description = f.Description,
                        CreationDate = f.CreationDate,
                        UpdateDate = f.UpdateDate
                    }).ToList();
                }
            }
            
            return plans;
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
            if (entity == null) return null;
            
            var plan = _mapper.Map<SubscriptionPlanSelectModel>(entity);
            
            // Se il piano Free non ha features, eredita quelle del Basic
            if (plan != null && plan.Name.Equals("Free", StringComparison.OrdinalIgnoreCase) 
                && (plan.Features == null || !plan.Features.Any()))
            {
                // Cerca il piano Basic
                var basicEntity = await _unitOfWork.SubscriptionPlanRepository.GetActivePlansAsync();
                var basicPlanEntity = basicEntity.FirstOrDefault(p => p.Name.Equals("Basic", StringComparison.OrdinalIgnoreCase));
                
                if (basicPlanEntity != null && basicPlanEntity.Features != null && basicPlanEntity.Features.Any())
                {
                    // Copia le features del Basic al Free, ma mantieni il SubscriptionPlanId del Free
                    plan.Features = basicPlanEntity.Features.Select(f => new Models.SubscriptionFeatureModels.SubscriptionFeatureSelectModel
                    {
                        Id = f.Id,
                        SubscriptionPlanId = plan.Id, // Usa l'ID del Free
                        FeatureName = f.FeatureName,
                        FeatureValue = f.FeatureValue,
                        Description = f.Description,
                        CreationDate = f.CreationDate,
                        UpdateDate = f.UpdateDate
                    }).ToList();
                }
            }
            
            return plan;
        }
    }
}
