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
        /// <param name="expand">Campi da espandere (es. "invoice")</param>
        /// <returns>Payment Intent</returns>
        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, List<string>? expand = null);

        /// <summary>
        /// Aggiorna i metadata di un Payment Intent (es. per il PI della prima invoice di una subscription, che non eredita i metadata dalla subscription).
        /// </summary>
        Task<PaymentIntent> UpdatePaymentIntentMetadataAsync(string paymentIntentId, Dictionary<string, string> metadata);

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
        /// Recupera una Subscription Stripe tramite ID
        /// </summary>
        Task<Subscription> GetSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Recupera una Invoice Stripe tramite ID
        /// </summary>
        Task<Invoice> GetInvoiceAsync(string invoiceId);

        /// <summary>
        /// Cancella una Subscription Stripe
        /// </summary>
        /// <param name="subscriptionId">ID della subscription</param>
        /// <returns>Subscription cancellata</returns>
        Task<Subscription> CancelSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Imposta o rimuove cancel_at_period_end sulla subscription Stripe (disattiva/riattiva rinnovo automatico).
        /// </summary>
        /// <param name="subscriptionId">ID della subscription Stripe</param>
        /// <param name="cancelAtPeriodEnd">true = non rinnovare a scadenza; false = rinnova normalmente</param>
        Task<Subscription> SetCancelAtPeriodEndAsync(string subscriptionId, bool cancelAtPeriodEnd);

        /// <summary>
        /// Crea un credito sul customer balance Stripe (riduce le prossime fatture).
        /// </summary>
        /// <param name="customerId">ID del customer Stripe</param>
        /// <param name="amountInCents">Importo di credito in centesimi (sempre positivo; il servizio converte in negativo per Stripe)</param>
        /// <param name="currency">Valuta (es. eur)</param>
        /// <param name="description">Descrizione per l'utente (es. "Credito residuo abbonamento – X giorni")</param>
        /// <param name="metadata">Metadata opzionali</param>
        /// <returns>Customer balance transaction creata</returns>
        Task<CustomerBalanceTransaction> CreateCustomerCreditAsync(string customerId, long amountInCents, string currency, string description, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Crea un SetupIntent per salvare un nuovo metodo di pagamento sul customer (senza addebitare).
        /// </summary>
        /// <param name="customerId">ID del customer Stripe</param>
        /// <param name="metadata">Metadati opzionali</param>
        /// <returns>SetupIntent con ClientSecret per il frontend</returns>
        Task<SetupIntent> CreateSetupIntentAsync(string customerId, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Imposta il metodo di pagamento predefinito per un customer (fatture future).
        /// </summary>
        Task SetDefaultPaymentMethodForCustomerAsync(string customerId, string paymentMethodId);

        /// <summary>
        /// Imposta il metodo di pagamento predefinito per una subscription (rinnovi).
        /// </summary>
        Task SetDefaultPaymentMethodForSubscriptionAsync(string subscriptionId, string paymentMethodId);

        /// <summary>
        /// Processa un evento webhook di Stripe
        /// </summary>
        /// <param name="json">Body JSON del webhook</param>
        /// <param name="stripeSignature">Header Stripe-Signature</param>
        /// <returns>Evento Stripe</returns>
        Event ConstructWebhookEvent(string json, string stripeSignature);
    }
}

