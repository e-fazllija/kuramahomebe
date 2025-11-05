namespace BackEnd.Models.SubscriptionLimitModels
{
    /// <summary>
    /// Response model per la verifica di compatibilità del downgrade
    /// </summary>
    public class DowngradeCompatibilityResponse
    {
        /// <summary>
        /// Indica se il downgrade è possibile senza problemi
        /// </summary>
        public bool CanDowngrade { get; set; }

        /// <summary>
        /// Piano di destinazione verificato
        /// </summary>
        public int TargetPlanId { get; set; }

        /// <summary>
        /// Nome del piano di destinazione
        /// </summary>
        public string TargetPlanName { get; set; } = null!;

        /// <summary>
        /// Lista delle features con confronto limiti vs utilizzo attuale
        /// </summary>
        public List<FeatureCompatibilityItem> Features { get; set; } = new List<FeatureCompatibilityItem>();

        /// <summary>
        /// Numero totale di limiti superati
        /// </summary>
        public int ExceededLimitsCount { get; set; }

        /// <summary>
        /// Messaggio generale per l'utente
        /// </summary>
        public string? Message { get; set; }
    }

    /// <summary>
    /// Item singolo feature per il confronto compatibilità
    /// </summary>
    public class FeatureCompatibilityItem
    {
        /// <summary>
        /// Nome della feature (es: "max_properties")
        /// </summary>
        public string FeatureName { get; set; } = null!;

        /// <summary>
        /// Descrizione leggibile della feature (es: "Immobili")
        /// </summary>
        public string FeatureDisplayName { get; set; } = null!;

        /// <summary>
        /// Limite del nuovo piano (null se unlimited)
        /// </summary>
        public int? NewPlanLimit { get; set; }

        /// <summary>
        /// Utilizzo corrente dell'utente
        /// </summary>
        public int CurrentUsage { get; set; }

        /// <summary>
        /// Indica se il limite viene superato
        /// </summary>
        public bool IsExceeded { get; set; }

        /// <summary>
        /// Messaggio descrittivo
        /// </summary>
        public string Message { get; set; } = null!;
    }
}





