using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Models.SubscriptionFeatureModels;

namespace BackEnd.Services.BusinessServices
{
    public class SubscriptionFeatureServices : ISubscriptionFeatureServices
    {
        private readonly ISubscriptionFeatureRepository _repository;
        private readonly IMapper _mapper;

        public SubscriptionFeatureServices(ISubscriptionFeatureRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SubscriptionFeatureSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<SubscriptionFeatureSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<SubscriptionFeatureSelectModel>> GetByPlanIdAsync(int planId)
        {
            var entities = await _repository.GetFeaturesByPlanIdAsync(planId);
            return _mapper.Map<IEnumerable<SubscriptionFeatureSelectModel>>(entities);
        }

        public async Task<SubscriptionFeatureSelectModel> CreateAsync(SubscriptionFeatureCreateModel model)
        {
            var entity = _mapper.Map<SubscriptionFeature>(model);
            var createdEntity = await _repository.CreateAsync(entity);
            return _mapper.Map<SubscriptionFeatureSelectModel>(createdEntity);
        }

        public async Task<SubscriptionFeatureSelectModel?> UpdateAsync(SubscriptionFeatureUpdateModel model)
        {
            var existingEntity = await _repository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<SubscriptionFeatureSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> DeleteByPlanIdAsync(int planId)
        {
            return await _repository.DeleteFeaturesByPlanIdAsync(planId);
        }

        public async Task<IEnumerable<SubscriptionFeatureSelectModel>> CreateMultipleAsync(IEnumerable<SubscriptionFeatureCreateModel> models)
        {
            var entities = _mapper.Map<IEnumerable<SubscriptionFeature>>(models);
            var createdEntities = new List<SubscriptionFeature>();
            
            foreach (var entity in entities)
            {
                var created = await _repository.CreateAsync(entity);
                createdEntities.Add(created);
            }
            
            return _mapper.Map<IEnumerable<SubscriptionFeatureSelectModel>>(createdEntities);
        }
    }
}
