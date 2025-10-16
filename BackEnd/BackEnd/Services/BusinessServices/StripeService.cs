using BackEnd.Interfaces.IBusinessServices;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace BackEnd.Services.BusinessServices
{
    public class StripeService : IStripeService
    {
        private readonly string _secretKey;
        private readonly string _webhookSecret;

        public StripeService(IConfiguration configuration)
        {
            // Leggi la chiave da appsettings o KeyVault
            _secretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey");
            _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
            
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, string email, Dictionary<string, string>? metadata = null)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency.ToLower(),
                ReceiptEmail = email,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }

        public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.ConfirmAsync(paymentIntentId);
        }

        public async Task<Stripe.Customer> CreateOrGetCustomerAsync(string email, string? name = null, Dictionary<string, string>? metadata = null)
        {
            var service = new CustomerService();

            // Cerca se esiste già un customer con questa email
            var searchOptions = new CustomerSearchOptions
            {
                Query = $"email:'{email}'",
            };

            var customers = await service.SearchAsync(searchOptions);
            
            if (customers.Data.Any())
            {
                return customers.Data.First();
            }

            // Se non esiste, crealo
            var createOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            return await service.CreateAsync(createOptions);
        }

        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null)
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" },
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new SubscriptionService();
            return await service.CreateAsync(options);
        }

        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            return await service.CancelAsync(subscriptionId);
        }

        public Event ConstructWebhookEvent(string json, string stripeSignature)
        {
            if (string.IsNullOrEmpty(_webhookSecret))
            {
                throw new InvalidOperationException("Webhook secret non configurato");
            }

            // Disabilita il controllo della versione API per evitare errori di mismatch
            // tra la versione API di Stripe e quella della libreria Stripe.net
            return EventUtility.ConstructEvent(
                json, 
                stripeSignature, 
                _webhookSecret, 
                throwOnApiVersionMismatch: false
            );
        }
    }
}

