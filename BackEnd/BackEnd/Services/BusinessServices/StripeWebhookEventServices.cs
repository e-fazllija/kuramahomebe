using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.StripeWebhookEventModels;

namespace BackEnd.Services.BusinessServices
{
    public class StripeWebhookEventServices : IStripeWebhookEventServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StripeWebhookEventServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StripeWebhookEventSelectModel?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.StripeWebhookEventRepository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<StripeWebhookEventSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<StripeWebhookEventSelectModel>> GetAllAsync()
        {
            var entities = await _unitOfWork.StripeWebhookEventRepository.GetListAsync();
            return _mapper.Map<IEnumerable<StripeWebhookEventSelectModel>>(entities);
        }

        public async Task<StripeWebhookEventSelectModel> CreateAsync(StripeWebhookEventCreateModel model)
        {
            var entity = _mapper.Map<StripeWebhookEvent>(model);
            var createdEntity = await _unitOfWork.StripeWebhookEventRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<StripeWebhookEventSelectModel>(createdEntity);
        }

        public async Task<StripeWebhookEventSelectModel?> UpdateAsync(StripeWebhookEventUpdateModel model)
        {
            var existingEntity = await _unitOfWork.StripeWebhookEventRepository.GetByIdAsync(model.Id);
            if (existingEntity == null) return null;

            _mapper.Map(model, existingEntity);
            var updatedEntity = await _unitOfWork.StripeWebhookEventRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<StripeWebhookEventSelectModel>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.StripeWebhookEventRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.StripeWebhookEventRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<StripeWebhookEventSelectModel?> GetByEventIdAsync(string eventId)
        {
            var entity = await _unitOfWork.StripeWebhookEventRepository.GetByEventIdAsync(eventId);
            return entity != null ? _mapper.Map<StripeWebhookEventSelectModel>(entity) : null;
        }

        public async Task<IEnumerable<StripeWebhookEventSelectModel>> GetUnprocessedEventsAsync()
        {
            var entities = await _unitOfWork.StripeWebhookEventRepository.GetUnprocessedEventsAsync();
            return _mapper.Map<IEnumerable<StripeWebhookEventSelectModel>>(entities);
        }

        public async Task<bool> IsEventProcessedAsync(string eventId)
        {
            return await _unitOfWork.StripeWebhookEventRepository.IsEventProcessedAsync(eventId);
        }

        public async Task<bool> MarkAsProcessedAsync(int id)
        {
            var result = await _unitOfWork.StripeWebhookEventRepository.MarkAsProcessedAsync(id);
            await _unitOfWork.SaveAsync();
            return result;
        }

        public async Task<bool> ProcessWebhookEventAsync(string eventId, string eventType, string data)
        {
            if (await IsEventProcessedAsync(eventId))
                return true;

            var eventRecord = await CreateAsync(new StripeWebhookEventCreateModel
            {
                EventId = eventId,
                Type = eventType,
                Data = data,
                Processed = false
            });

            return eventRecord != null && await MarkAsProcessedAsync(eventRecord.Id);
        }
    }
}
