using System;
using System.Linq;
using AutoMapper;
using BackEnd.Entities;
using BackEnd.Helpers;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionFeatureModels;
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
            if (entity == null) return null;
            
            var subscription = _mapper.Map<UserSubscriptionSelectModel>(entity);
            await EnsurePlanFeaturesFromBaseMonthlyAsync(subscription);
            return subscription;
        }

        public async Task<IEnumerable<UserSubscriptionSelectModel>> GetUserSubscriptionsAsync(string userId)
        {
            var entities = await _unitOfWork.UserSubscriptionRepository.GetUserSubscriptionsAsync(userId);
            var subscriptions = _mapper.Map<IEnumerable<UserSubscriptionSelectModel>>(entities).ToList();
            foreach (var sub in subscriptions)
                await EnsurePlanFeaturesFromBaseMonthlyAsync(sub);
            return subscriptions;
        }

        public async Task<UserSubscriptionSelectModel?> GetActiveUserSubscriptionAsync(string userId, string? agencyId = null)
        {
            var entity = await _unitOfWork.UserSubscriptionRepository.GetActiveUserSubscriptionAsync(userId, agencyId);
            if (entity == null) return null;
            
            var subscription = _mapper.Map<UserSubscriptionSelectModel>(entity);
            await EnsurePlanFeaturesFromBaseMonthlyAsync(subscription);
            return subscription;
        }

        /// <summary>
        /// Per Free senza features usa quelle del Basic mensile.
        /// Per prepagate (Basic/Pro/Premium 3-6-12 mesi) usa le feature del piano base mensile corrispondente.
        /// </summary>
        private async Task EnsurePlanFeaturesFromBaseMonthlyAsync(UserSubscriptionSelectModel? subscription)
        {
            if (subscription?.SubscriptionPlan == null) return;

            var plan = subscription.SubscriptionPlan;
            var name = plan.Name ?? "";

            // Free senza features → eredita da Basic mensile
            if (name.Equals("Free", StringComparison.OrdinalIgnoreCase))
            {
                if (plan.Features != null && plan.Features.Any()) return;
                var basePlan = await GetBaseMonthlyPlanAsync("Basic");
                if (basePlan?.Features != null && basePlan.Features.Any())
                    plan.Features = CopyFeatures(basePlan.Features, plan.Id);
                return;
            }

            // Prepagate: nome tipo "Basic 3 months", "Pro 6 months", "Premium 12 months" → usa il piano base mensile
            string? baseName = null;
            if (name.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) baseName = "Basic";
            else if (name.StartsWith("Pro ", StringComparison.OrdinalIgnoreCase)) baseName = "Pro";
            else if (name.StartsWith("Premium ", StringComparison.OrdinalIgnoreCase)) baseName = "Premium";

            if (baseName != null)
            {
                var basePlan = await GetBaseMonthlyPlanAsync(baseName);
                if (basePlan?.Features != null && basePlan.Features.Any())
                    plan.Features = CopyFeatures(basePlan.Features, plan.Id);
            }
        }

        private async Task<Models.SubscriptionPlanModels.SubscriptionPlanSelectModel?> GetBaseMonthlyPlanAsync(string baseName)
        {
            var allEntities = await _unitOfWork.SubscriptionPlanRepository.GetActivePlansAsync();
            var baseEntity = allEntities.FirstOrDefault(p =>
                p.Name != null && p.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)
                && (p.BillingPeriod ?? "").Trim().Equals("monthly", StringComparison.OrdinalIgnoreCase));
            return baseEntity == null ? null : _mapper.Map<Models.SubscriptionPlanModels.SubscriptionPlanSelectModel>(baseEntity);
        }

        private static List<SubscriptionFeatureSelectModel> CopyFeatures(
            List<SubscriptionFeatureSelectModel> source,
            int targetPlanId)
        {
            return source.Select(f => new SubscriptionFeatureSelectModel
            {
                Id = f.Id,
                SubscriptionPlanId = targetPlanId,
                FeatureName = f.FeatureName,
                FeatureValue = f.FeatureValue,
                Description = f.Description,
                CreationDate = f.CreationDate,
                UpdateDate = f.UpdateDate
            }).ToList();
        }

        public async Task<UserSubscriptionSelectModel> CreateAsync(UserSubscriptionCreateModel model)
        {
            var entity = _mapper.Map<UserSubscription>(model);
            
            // Imposta CreationDate e UpdateDate
            var now = DateTime.UtcNow;
            if (entity.CreationDate == default(DateTime) || entity.CreationDate == new DateTime(1, 1, 1))
            {
                entity.CreationDate = now;
            }
            entity.UpdateDate = now;
            
            // AutoRenew viene mappato automaticamente da AutoMapper dal modello
            // Il modello ha default = true, quindi dovrebbe essere già impostato
            
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
            
            var planName = activeSubscription.SubscriptionPlan?.Name;
            var status = activeSubscription.Status?.ToLowerInvariant() ?? "";
            // Includi anche piani prepagati (es. "Premium 3 Months", "Premium 12 Months")
            return SubscriptionPlanTierHelper.IsPremiumPlanName(planName) && status == "active";
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