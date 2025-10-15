using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Models.StripeWebhookEventModels;

namespace BackEnd.Services.BusinessServices
{
    public class StripeWebhookEventServices : IStripeWebhookEventServices
    {
        private readonly IStripeWebhookEventRepository _repository;
        private readonly IMapper _mapper;

        public StripeWebhookEventServices(IStripeWebhookEventRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StripeWebhookEventSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<StripeWebhookEventSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<StripeWebhookEventSelectModel>> GetAllAsync()
        {
            var entities = await _repository.GetListAsync();
            return _mapper.Map<IEnumerable<StripeWebhookEventSelectModel>>(entities);
        }

        public async Task<StripeWebhookEventSelectModel> CreateAsync(StripeWebhookEventCreateModel model)
        {
            var entity = _mapper.Map<StripeWebhookEvent>(model);
            var createdEntity = await _repository.CreateAsync(entity);
            return _mapper.Map<StripeWebhookEventSelectModel>(createdEntity);
        }

        public async Task<StripeWebhookEventSelectModel?> UpdateAsync(StripeWebhookEventUpdateModel model)
        {
            var existingEntity = await _repository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<StripeWebhookEventSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<StripeWebhookEventSelectModel?> GetByEventIdAsync(string eventId)
        {
            var entity = await _repository.GetByEventIdAsync(eventId);
            return entity != null ? _mapper.Map<StripeWebhookEventSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<StripeWebhookEventSelectModel>> GetUnprocessedEventsAsync()
        {
            var entities = await _repository.GetUnprocessedEventsAsync();
            return _mapper.Map<IEnumerable<StripeWebhookEventSelectModel>>(entities);
        }

        public async Task<bool> IsEventProcessedAsync(string eventId)
        {
            return await _repository.IsEventProcessedAsync(eventId);
        }

        public async Task<bool> MarkAsProcessedAsync(int id)
        {
            return await _repository.MarkAsProcessedAsync(id);
        }

        public async Task<bool> ProcessWebhookEventAsync(string eventId, string eventType, string data)
        {
            // Verifica se l'evento è già stato processato
            if (await IsEventProcessedAsync(eventId))
                return true;

            // Crea il record dell'evento
            var createModel = new StripeWebhookEventCreateModel
            {
                EventId = eventId,
                Type = eventType,
                Data = data,
                Processed = false
            };

            var eventRecord = await CreateAsync(createModel);
            
            // Qui implementeresti la logica specifica per processare ogni tipo di evento
            // Per ora segniamo come processato
            if (eventRecord != null)
            {
                return await MarkAsProcessedAsync(eventRecord.Id);
            }

            return false;
        }
    }
}
