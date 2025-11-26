using Stripe;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IStripeService
    {
        /// <summary>
        /// Crea un Payment Intent di Stripe per processare un pagamento
        /// </summary>
        /// <param name="amount">Importo in centesimi</param>
        /// <param name="currency">Valuta (es. eur)</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="metadata">Metadati aggiuntivi</param>
        /// <returns>Payment Intent creato</returns>
        Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, string email, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Recupera un Payment Intent tramite ID
        /// </summary>
        /// <param name="paymentIntentId">ID del Payment Intent</param>
        /// <returns>Payment Intent</returns>
        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Conferma un Payment Intent
        /// </summary>
        /// <param name="paymentIntentId">ID del Payment Intent</param>
        /// <returns>Payment Intent confermato</returns>
        Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Crea o recupera un Customer Stripe
        /// </summary>
        /// <param name="email">Email del cliente</param>
        /// <param name="name">Nome del cliente</param>
        /// <param name="metadata">Metadati aggiuntivi</param>
        /// <returns>Customer Stripe</returns>
        Task<Customer> CreateOrGetCustomerAsync(string email, string? name = null, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Crea una Subscription Stripe
        /// </summary>
        /// <param name="customerId">ID del customer Stripe</param>
        /// <param name="priceId">ID del prezzo/piano Stripe</param>
        /// <param name="metadata">Metadati aggiuntivi</param>
        /// <returns>Subscription creata</returns>
        Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Cancella una Subscription Stripe
        /// </summary>
        /// <param name="subscriptionId">ID della subscription</param>
        /// <returns>Subscription cancellata</returns>
        Task<Subscription> CancelSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Processa un evento webhook di Stripe
        /// </summary>
        /// <param name="json">Body JSON del webhook</param>
        /// <param name="stripeSignature">Header Stripe-Signature</param>
        /// <returns>Evento Stripe</returns>
        Event ConstructWebhookEvent(string json, string stripeSignature);
    }
}

