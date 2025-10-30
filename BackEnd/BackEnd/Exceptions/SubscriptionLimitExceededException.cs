using BackEnd.Models.SubscriptionLimitModels;

namespace BackEnd.Exceptions
{
    /// <summary>
    /// Eccezione lanciata quando un limite di subscription è stato raggiunto
    /// </summary>
    public class SubscriptionLimitExceededException : Exception
    {
        /// <summary>
        /// Dettagli del limite raggiunto
        /// </summary>
        public SubscriptionLimitStatusResponse Result { get; }

        public SubscriptionLimitExceededException(SubscriptionLimitStatusResponse result)
            : base(result.Message ?? $"Limite raggiunto per la feature: {result.FeatureName}")
        {
            Result = result;
        }
    }
}




