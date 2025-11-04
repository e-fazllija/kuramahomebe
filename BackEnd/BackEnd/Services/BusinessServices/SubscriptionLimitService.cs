using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionLimitModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BackEnd.Services.BusinessServices
{
    public class SubscriptionLimitService : ISubscriptionLimitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;
        private readonly UserManager<Entities.ApplicationUser> _userManager;

        public SubscriptionLimitService(
            IUnitOfWork unitOfWork,
            IUserSubscriptionServices userSubscriptionServices,
            ISubscriptionPlanServices subscriptionPlanServices,
            UserManager<Entities.ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
            _userManager = userManager;
        }

        public async Task<SubscriptionLimitStatusResponse> CheckFeatureLimitAsync(
            string userId,
            string featureName,
            string? agencyId = null)
        {
            // 1. Recupera subscription attiva (risale gerarchia fino all'Admin)
            var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, agencyId);

            if (subscription == null)
            {
                // Nessuna subscription = nessun limite, può procedere
                return new SubscriptionLimitStatusResponse
                {
                    CanProceed = true,
                    FeatureName = featureName,
                    Limit = null,
                    CurrentUsage = 0,
                    LimitReached = false,
                    Message = "Nessun limite configurato"
                };
            }

            // 2. Leggi feature dal piano (dal DB) - confronto case-insensitive e senza underscore
            // Cerca sia "MaxAgencies" che "max_agencies" che "Max_Agencies"
            var feature = subscription.SubscriptionPlan?.Features
                ?.FirstOrDefault(f => 
                    // Confronto diretto case-insensitive
                    string.Equals(f.FeatureName, featureName, StringComparison.OrdinalIgnoreCase) ||
                    // Confronto normalizzato (rimuove underscore)
                    string.Equals(f.FeatureName.Replace("_", "").Replace("-", ""), featureName.Replace("_", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase) ||
                    // Confronto con formato PascalCase normalizzato
                    string.Equals(f.FeatureName, NormalizeFeatureName(featureName), StringComparison.OrdinalIgnoreCase));

            if (feature == null)
            {
                // Feature non trovata - log per debug
                var availableFeatures = subscription.SubscriptionPlan?.Features?.Select(f => f.FeatureName).ToList() ?? new List<string>();
                var featuresList = string.Join(", ", availableFeatures);
                
                // Feature non configurata = nessun limite
                return new SubscriptionLimitStatusResponse
                {
                    CanProceed = true,
                    FeatureName = featureName,
                    Limit = null,
                    CurrentUsage = 0,
                    LimitReached = false,
                    Message = $"Limite non configurato per '{featureName}'. Features disponibili: [{featuresList}]"
                };
            }
            
            if (string.IsNullOrEmpty(feature.FeatureValue))
            {
                // Feature trovata ma senza valore
                return new SubscriptionLimitStatusResponse
                {
                    CanProceed = true,
                    FeatureName = featureName,
                    Limit = null,
                    CurrentUsage = 0,
                    LimitReached = false,
                    Message = $"Feature '{feature.FeatureName}' trovata ma FeatureValue è vuoto"
                };
            }

            // 3. Parsa FeatureValue
            int? limit = ParseLimit(feature.FeatureValue);

            // 4. Identifica chi ha la subscription (Admin root per conteggio gerarchico)
            string subscriptionOwnerId = subscription.UserId;

            // 5. Conta entità esistenti nella gerarchia
            int currentUsage = await CountEntitiesInHierarchyAsync(featureName, subscriptionOwnerId);

            // 6. Verifica limite
            bool limitReached = limit.HasValue && currentUsage >= limit.Value;
            int? remaining = limit.HasValue ? Math.Max(0, limit.Value - currentUsage) : null;

            // 7. Genera messaggio
            string message = GenerateMessage(featureName, limit, currentUsage, limitReached);

            return new SubscriptionLimitStatusResponse
            {
                CanProceed = !limitReached,
                FeatureName = featureName,
                Limit = feature.FeatureValue,
                CurrentUsage = currentUsage,
                LimitReached = limitReached,
                Remaining = remaining,
                Message = message
            };
        }

        public async Task<Dictionary<string, SubscriptionLimitStatusResponse>> GetAllLimitsAsync(
            string userId,
            string? agencyId = null)
        {
            var result = new Dictionary<string, SubscriptionLimitStatusResponse>();

            // Recupera subscription attiva
            var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, agencyId);

            if (subscription == null || subscription.SubscriptionPlan?.Features == null)
                return result;

            // Per ogni feature del piano, verifica il limite
            foreach (var feature in subscription.SubscriptionPlan.Features)
            {
                var limitStatus = await CheckFeatureLimitAsync(userId, feature.FeatureName, agencyId);
                result[feature.FeatureName] = limitStatus;
            }

            return result;
        }

        /// <summary>
        /// Conta entità esistenti nella gerarchia dell'Admin
        /// </summary>
        private async Task<int> CountEntitiesInHierarchyAsync(string featureName, string adminRootId)
        {
            // Normalizza il nome per il confronto (supporta sia "max_agencies" che "MaxAgencies")
            var normalized = NormalizeFeatureName(featureName);
            var lowerFeatureName = featureName.ToLower();
            
            // Confronta sia con il formato normalizzato che con quello originale
            if (normalized.Equals("MaxAgencies", StringComparison.OrdinalIgnoreCase) || 
                lowerFeatureName == "max_agencies")
                return await CountAgenciesAsync(adminRootId);
            
            if (normalized.Equals("MaxProperties", StringComparison.OrdinalIgnoreCase) || 
                lowerFeatureName == "max_properties")
                return await CountPropertiesAsync(adminRootId);
            
            if (normalized.Equals("MaxAgents", StringComparison.OrdinalIgnoreCase) || 
                lowerFeatureName == "max_agents")
                return await CountAgentsAsync(adminRootId);
            
            if (normalized.Equals("MaxCustomers", StringComparison.OrdinalIgnoreCase) || 
                lowerFeatureName == "max_customers")
                return await CountCustomersAsync(adminRootId);
            
            if (normalized.Equals("MaxRequests", StringComparison.OrdinalIgnoreCase) || 
                lowerFeatureName == "max_requests")
                return await CountRequestsAsync(adminRootId);
            
            return 0;
        }

        /// <summary>
        /// Conta agenzie create dall'Admin
        /// </summary>
        private async Task<int> CountAgenciesAsync(string adminId)
        {
            var agencies = await _userManager.GetUsersInRoleAsync("Agency");
            return agencies.Count(u => u.AdminId == adminId);
        }

        /// <summary>
        /// Conta tutti gli immobili nella gerarchia dell'Admin (Admin + Agency + Agent)
        /// </summary>
        private async Task<int> CountPropertiesAsync(string adminId)
        {
            // Trova tutte le Agency dirette dell'Admin
            var directAgencies = await _unitOfWork.dbContext.Users
                .Where(u => u.AdminId == adminId)
                .Select(u => u.Id)
                .ToListAsync();

            // Conta immobili:
            // - Creati direttamente dall'Admin (AgentId == adminId)
            // - Creati da Agency dirette (AgentId in directAgencies)
            // - Creati da Agent di Agency dirette (Agent.AgencyId in directAgencies)
            return await _unitOfWork.dbContext.RealEstateProperties
                .Include(p => p.User)
                .Where(p => !p.Archived && (
                    p.UserId == adminId ||  // Admin diretto
                    directAgencies.Contains(p.UserId) ||  // Agency dirette
                    (p.User.AdminId != null && directAgencies.Contains(p.User.AdminId))  // Agent di Agency
                ))
                .CountAsync();
        }

        /// <summary>
        /// Conta tutti gli Agent in tutte le Agency dell'Admin
        /// </summary>
        private async Task<int> CountAgentsAsync(string adminId)
        {
            // Trova tutte le Agency dirette dell'Admin
            var directAgencies = await _unitOfWork.dbContext.Users
                .Where(u => u.AdminId == adminId)
                .Select(u => u.Id)
                .ToListAsync();

            // Conta Agent che hanno AgencyId nelle Agency dirette
            var agents = await _userManager.GetUsersInRoleAsync("Agent");
            return agents.Count(a => a.AdminId != null && directAgencies.Contains(a.AdminId));
        }

        /// <summary>
        /// Conta tutti i Customer nelle Agency dell'Admin
        /// </summary>
        private async Task<int> CountCustomersAsync(string adminId)
        {
            // Trova tutte le Agency dirette dell'Admin
            var directAgencies = await _unitOfWork.dbContext.Users
                .Where(u => u.AdminId == adminId)
                .Select(u => u.Id)
                .ToListAsync();

            return await _unitOfWork.dbContext.Customers
                .Where(c => c.UserId != null && directAgencies.Contains(c.UserId))
                .CountAsync();
        }

        /// <summary>
        /// Conta tutte le Request nelle Agency dell'Admin
        /// </summary>
        private async Task<int> CountRequestsAsync(string adminId)
        {
            // Trova tutte le Agency dirette dell'Admin
            var directAgencies = await _unitOfWork.dbContext.Users
                .Where(u => u.AdminId == adminId)
                .Select(u => u.Id)
                .ToListAsync();

            return await _unitOfWork.dbContext.Requests
                .Where(r => r.UserId != null && directAgencies.Contains(r.UserId) && !r.Archived)
                .CountAsync();
        }

        /// <summary>
        /// Converte FeatureValue in int? (null se unlimited)
        /// </summary>
        private int? ParseLimit(string? featureValue)
        {
            if (string.IsNullOrWhiteSpace(featureValue))
                return null;

            featureValue = featureValue.Trim().ToLower();

            if (featureValue == "unlimited" || featureValue == "-1")
                return null;

            if (int.TryParse(featureValue, out int limit))
                return limit;

            return null;
        }

        /// <summary>
        /// Genera messaggio descrittivo per l'utente
        /// </summary>
        private string GenerateMessage(string featureName, int? limit, int currentUsage, bool limitReached)
        {
            string entityName = GetEntityDisplayName(featureName);

            if (!limit.HasValue)
                return $"Puoi creare {entityName} illimitati. Utilizzo attuale: {currentUsage}";

            if (limitReached)
                return $"Hai raggiunto il limite di {limit.Value} {entityName}. Utilizzo attuale: {currentUsage}";

            int remaining = limit.Value - currentUsage;
            return $"Limite: {limit.Value} {entityName}. Utilizzo attuale: {currentUsage}. Rimanenti: {remaining}";
        }

        /// <summary>
        /// Normalizza il nome della feature per il confronto con il DB
        /// Converte "max_agencies" -> "MaxAgencies" per compatibilità
        /// </summary>
        private string NormalizeFeatureName(string featureName)
        {
            // Se già in formato PascalCase, restituisci così
            if (!featureName.Contains('_'))
                return featureName;

            // Converte "max_agencies" -> "MaxAgencies"
            return string.Join("", 
                featureName.Split('_')
                    .Select(part => char.ToUpper(part[0]) + part.Substring(1).ToLower())
            );
        }

        /// <summary>
        /// Restituisce nome visualizzabile dell'entità
        /// </summary>
        private string GetEntityDisplayName(string featureName)
        {
            // Normalizza per il confronto
            var normalized = NormalizeFeatureName(featureName);
            
            return normalized.ToLower() switch
            {
                "maxagencies" => "agenzie",
                "maxproperties" => "immobili",
                "maxagents" => "agenti",
                "maxcustomers" => "clienti",
                "maxrequests" => "richieste",
                _ => featureName.ToLower() switch
                {
                    "max_agencies" => "agenzie",
                    "max_properties" => "immobili",
                    "max_agents" => "agenti",
                    "max_customers" => "clienti",
                    "max_requests" => "richieste",
                    _ => "entità"
                }
            };
        }

        /// <summary>
        /// Verifica se il downgrade al piano specificato è possibile confrontando limiti vs utilizzo attuale
        /// </summary>
        public async Task<DowngradeCompatibilityResponse> CheckDowngradeCompatibilityAsync(
            string userId,
            int targetPlanId,
            string? agencyId = null)
        {
            var response = new DowngradeCompatibilityResponse
            {
                TargetPlanId = targetPlanId,
                CanDowngrade = true,
                ExceededLimitsCount = 0,
                Features = new List<FeatureCompatibilityItem>()
            };

            // Recupera il piano di destinazione
            var targetPlan = await _subscriptionPlanServices.GetPlanWithFeaturesAsync(targetPlanId);
            if (targetPlan == null)
            {
                response.Message = "Piano di destinazione non trovato";
                response.CanDowngrade = false;
                return response;
            }

            response.TargetPlanName = targetPlan.Name;

            // Se il piano non ha features, il downgrade è sempre possibile
            if (targetPlan.Features == null || !targetPlan.Features.Any())
            {
                response.Message = "Il piano di destinazione non ha limiti configurati";
                return response;
            }

            // Recupera l'abbonamento attuale per identificare l'Admin root (subscriptionOwnerId)
            var currentSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, agencyId);
            string subscriptionOwnerId = userId; // Default all'utente stesso
            if (currentSubscription != null)
            {
                subscriptionOwnerId = currentSubscription.UserId;
            }

            // Per ogni feature del piano di destinazione, verifica l'utilizzo attuale
            foreach (var feature in targetPlan.Features)
            {
                var featureItem = new FeatureCompatibilityItem
                {
                    FeatureName = feature.FeatureName,
                    FeatureDisplayName = GetEntityDisplayName(feature.FeatureName)
                };

                // Parsa il limite del nuovo piano
                int? newLimit = ParseLimit(feature.FeatureValue);
                featureItem.NewPlanLimit = newLimit;

                // Conta l'utilizzo corrente nella gerarchia dell'Admin root
                int currentUsage = await CountEntitiesInHierarchyAsync(feature.FeatureName, subscriptionOwnerId);
                featureItem.CurrentUsage = currentUsage;

                // Verifica se il limite viene superato
                if (newLimit.HasValue && currentUsage > newLimit.Value)
                {
                    featureItem.IsExceeded = true;
                    featureItem.Message = $"Limite: {newLimit.Value} {featureItem.FeatureDisplayName}, Utilizzo attuale: {currentUsage} (superato di {currentUsage - newLimit.Value})";
                    response.ExceededLimitsCount++;
                    response.CanDowngrade = false;
                }
                else if (newLimit.HasValue)
                {
                    featureItem.IsExceeded = false;
                    int remaining = newLimit.Value - currentUsage;
                    featureItem.Message = $"Limite: {newLimit.Value} {featureItem.FeatureDisplayName}, Utilizzo attuale: {currentUsage}, Rimanenti: {remaining}";
                }
                else
                {
                    // Unlimited
                    featureItem.IsExceeded = false;
                    featureItem.Message = $"Illimitato - Utilizzo attuale: {currentUsage} {featureItem.FeatureDisplayName}";
                }

                response.Features.Add(featureItem);
            }

            // Genera messaggio generale
            if (response.CanDowngrade)
            {
                response.Message = $"Il downgrade al piano {response.TargetPlanName} è possibile. Tutti i limiti sono rispettati.";
            }
            else
            {
                response.Message = $"Il downgrade al piano {response.TargetPlanName} non è possibile. Hai {response.ExceededLimitsCount} limite/i superato/i. Elimina alcune risorse prima di procedere.";
            }

            return response;
        }
    }
}

