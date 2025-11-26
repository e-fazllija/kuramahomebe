using BackEnd.Models.SubscriptionLimitModels;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Interfaces.IBusinessServices
{
    /// <summary>
    /// Interfaccia per il servizio di verifica limiti delle subscription
    /// </summary>
    public interface ISubscriptionLimitService
    {
        /// <summary>
        /// Verifica se l'utente può procedere con la creazione dell'entità specificata
        /// </summary>
        /// <param name="userId">ID dell'utente che sta creando</param>
        /// <param name="featureName">Nome della feature da verificare (es: "max_agencies", "max_properties")</param>
        /// <param name="agencyId">ID dell'agenzia (opzionale, per risalita gerarchica)</param>
        /// <returns>Status del limite con informazioni dettagliate</returns>
        Task<SubscriptionLimitStatusResponse> CheckFeatureLimitAsync(string userId, string featureName, string? agencyId = null);

        /// <summary>
        /// Recupera tutti i limiti dello stato per l'utente specificato
        /// </summary>
        /// <param name="userId">ID dell'utente</param>
        /// <param name="agencyId">ID dell'agenzia (opzionale)</param>
        /// <returns>Dictionary con tutti i limiti (chiave = featureName)</returns>
        Task<Dictionary<string, SubscriptionLimitStatusResponse>> GetAllLimitsAsync(string userId, string? agencyId = null);

        /// <summary>
        /// Verifica se il downgrade al piano specificato è possibile confrontando limiti vs utilizzo attuale
        /// </summary>
        /// <param name="userId">ID dell'utente</param>
        /// <param name="targetPlanId">ID del piano di destinazione</param>
        /// <param name="agencyId">ID dell'agenzia (opzionale)</param>
        /// <returns>Response con compatibilità e dettagli delle features</returns>
        Task<DowngradeCompatibilityResponse> CheckDowngradeCompatibilityAsync(string userId, int targetPlanId, string? agencyId = null);

        /// <summary>
        /// Verifica se l'export è abilitato per l'utente
        /// </summary>
        /// <param name="userId">ID dell'utente</param>
        /// <param name="agencyId">ID dell'agenzia (opzionale)</param>
        /// <returns>True se export è abilitato, false altrimenti</returns>
        Task<bool> IsExportEnabledAsync(string userId, string? agencyId = null);

        /// <summary>
        /// Registra un export effettuato dall'utente
        /// </summary>
        /// <param name="userId">ID dell'utente (Admin root)</param>
        /// <param name="exportType">Tipo export ("excel", "csv")</param>
        /// <param name="entityType">Tipo entità esportata (opzionale)</param>
        Task RecordExportAsync(string userId, string exportType, string? entityType = null);

        /// <summary>
        /// Controlla se i permessi per l'esportazione sono rispettati
        /// </summary>
        /// <param name="userId">ID dell'utente (Admin root)</param>
        Task<bool?> EnsureExportPermissions(string userId);
    }
}




