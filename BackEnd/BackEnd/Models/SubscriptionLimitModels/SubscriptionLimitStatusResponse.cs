namespace BackEnd.Models.SubscriptionLimitModels
{
    /// <summary>
    /// Response model per la verifica dei limiti di subscription
    /// </summary>
    public class SubscriptionLimitStatusResponse
    {
        /// <summary>
        /// Indica se l'utente può procedere con l'operazione
        /// </summary>
        public bool CanProceed { get; set; }

        /// <summary>
        /// Nome della feature verificata (es: "max_agencies", "max_properties")
        /// </summary>
        public string FeatureName { get; set; } = null!;

        /// <summary>
        /// Valore del limite originale dal DB (es: "5", "unlimited", "-1")
        /// </summary>
        public string? Limit { get; set; }

        /// <summary>
        /// Utilizzo corrente dell'entità nel sistema
        /// </summary>
        public int CurrentUsage { get; set; }

        /// <summary>
        /// Indica se il limite è stato raggiunto
        /// </summary>
        public bool LimitReached { get; set; }

        /// <summary>
        /// Limiti rimanenti (null se unlimited)
        /// </summary>
        public int? Remaining { get; set; }

        /// <summary>
        /// Messaggio descrittivo per l'utente
        /// </summary>
        public string? Message { get; set; }
    }
}




