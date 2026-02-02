using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using Stripe;

namespace BackEnd.Services.BusinessServices
{
    public class StripeService : IStripeService
    {
        private readonly string _secretKey;
        private readonly string _webhookSecret;

        public StripeService(IConfiguration configuration, IKeyVaultSecretProvider secretProvider)
        {
            var secretName = configuration["KeyVault:Secrets:StripeSecretKey"];
            _secretKey = secretProvider.GetSecret(secretName, "Stripe:SecretKey")
                ?? configuration["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe secret key non configurata.");

            var webhookSecretName = configuration["KeyVault:Secrets:StripeWebhookSecret"];
            _webhookSecret = secretProvider.GetSecret(webhookSecretName, "Stripe:WebhookSecret")
                ?? configuration["Stripe:WebhookSecret"]
                ?? string.Empty;
            
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

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, List<string>? expand = null)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentGetOptions();
            if (expand != null)
                foreach (var e in expand)
                    options.AddExpand(e);
            return await service.GetAsync(paymentIntentId, options);
        }

        public async Task<PaymentIntent> UpdatePaymentIntentMetadataAsync(string paymentIntentId, Dictionary<string, string> metadata)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentUpdateOptions { Metadata = metadata };
            return await service.UpdateAsync(paymentIntentId, options);
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

        public async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            return await service.GetAsync(subscriptionId);
        }

        public async Task<Invoice> GetInvoiceAsync(string invoiceId)
        {
            var service = new InvoiceService();
            return await service.GetAsync(invoiceId);
        }

        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            return await service.CancelAsync(subscriptionId);
        }

        public async Task<Subscription> SetCancelAtPeriodEndAsync(string subscriptionId, bool cancelAtPeriodEnd)
        {
            var service = new SubscriptionService();
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = cancelAtPeriodEnd
            };
            return await service.UpdateAsync(subscriptionId, options);
        }

        public async Task<SetupIntent> CreateSetupIntentAsync(string customerId, Dictionary<string, string>? metadata = null)
        {
            var options = new SetupIntentCreateOptions
            {
                Customer = customerId,
                Usage = "off_session",
                Metadata = metadata ?? new Dictionary<string, string>()
            };
            var service = new SetupIntentService();
            return await service.CreateAsync(options);
        }

        public async Task SetDefaultPaymentMethodForCustomerAsync(string customerId, string paymentMethodId)
        {
            var options = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId
                }
            };
            var service = new CustomerService();
            await service.UpdateAsync(customerId, options);
        }

        public async Task SetDefaultPaymentMethodForSubscriptionAsync(string subscriptionId, string paymentMethodId)
        {
            var options = new SubscriptionUpdateOptions
            {
                DefaultPaymentMethod = paymentMethodId
            };
            var service = new SubscriptionService();
            await service.UpdateAsync(subscriptionId, options);
        }

        public async Task<CustomerBalanceTransaction> CreateCustomerCreditAsync(string customerId, long amountInCents, string currency, string description, Dictionary<string, string>? metadata = null)
        {
            // Stripe: amount negativo = credito a favore del cliente (riduce le prossime fatture)
            // Riceviamo amountInCents positivo, convertiamo in negativo per Stripe
            var stripeAmount = -Math.Abs(amountInCents);
            var options = new CustomerBalanceTransactionCreateOptions
            {
                Amount = stripeAmount,
                Currency = currency.ToLower(),
                Description = description?.Length > 350 ? description.Substring(0, 350) : description
            };
            if (metadata != null && metadata.Count > 0)
            {
                options.Metadata = metadata;
            }
            var service = new CustomerBalanceTransactionService();
            return await service.CreateAsync(customerId, options);
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

