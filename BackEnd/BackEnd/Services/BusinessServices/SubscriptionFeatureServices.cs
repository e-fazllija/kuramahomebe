using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionFeatureModels;

namespace BackEnd.Services.BusinessServices
{
    public class SubscriptionFeatureServices : ISubscriptionFeatureServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionFeatureServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SubscriptionFeatureSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionFeatureRepository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<SubscriptionFeatureSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<SubscriptionFeatureSelectModel>> GetByPlanIdAsync(int planId)
        {
            var entities = await _unitOfWork.SubscriptionFeatureRepository.GetFeaturesByPlanIdAsync(planId);
            return _mapper.Map<IEnumerable<SubscriptionFeatureSelectModel>>(entities);
        }

        public async Task<SubscriptionFeatureSelectModel> CreateAsync(SubscriptionFeatureCreateModel model)
        {
            var entity = _mapper.Map<SubscriptionFeature>(model);
            var createdEntity = await _unitOfWork.SubscriptionFeatureRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<SubscriptionFeatureSelectModel>(createdEntity);
        }

        public async Task<SubscriptionFeatureSelectModel?> UpdateAsync(SubscriptionFeatureUpdateModel model)
        {
            var existingEntity = await _unitOfWork.SubscriptionFeatureRepository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _unitOfWork.SubscriptionFeatureRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<SubscriptionFeatureSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SubscriptionFeatureRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.SubscriptionFeatureRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteByPlanIdAsync(int planId)
        {
            var result = await _unitOfWork.SubscriptionFeatureRepository.DeleteFeaturesByPlanIdAsync(planId);
            await _unitOfWork.SaveAsync();
            return result;
        }

        public async Task<IEnumerable<SubscriptionFeatureSelectModel>> CreateMultipleAsync(IEnumerable<SubscriptionFeatureCreateModel> models)
        {
            var entities = _mapper.Map<IEnumerable<SubscriptionFeature>>(models);
            var createdEntities = new List<SubscriptionFeature>();
            
            foreach (var entity in entities)
            {
                var created = await _unitOfWork.SubscriptionFeatureRepository.CreateAsync(entity);
                createdEntities.Add(created);
            }
            
            await _unitOfWork.SaveAsync();
            return _mapper.Map<IEnumerable<SubscriptionFeatureSelectModel>>(createdEntities);
        }
    }
}
