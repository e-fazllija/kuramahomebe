using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Services.BusinessServices;

namespace BackEnd.Services
{
    public static class ServicesStartup
    {

      
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {

            builder.Services.AddSingleton<IKeyVaultSecretProvider, KeyVaultSecretProvider>();
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IStorageServices, StorageServices>();
            builder.Services.AddTransient<IPropertyStorageService, PropertyStorageService>();
            builder.Services.AddTransient<IMailService, MailService>();
            builder.Services.AddTransient<IGenericService, GenericService>();
            builder.Services.AddTransient<ICustomerServices, CustomerServices>();
            builder.Services.AddTransient<IRealEstatePropertyServices, RealEstatePropertyServices>();
            builder.Services.AddTransient<IRealEstatePropertyPhotoServices, RealEstatePropertyPhotoServices>();
            builder.Services.AddTransient<IRequestServices, RequestServices>();
            builder.Services.AddTransient<ICalendarServices, CalendarServices>();
            builder.Services.AddTransient<IDocumentsTabServices, DocumentsTabServices>();
            
            // Billing & Subscription Services
            builder.Services.AddTransient<IStripeService, StripeService>();
            builder.Services.AddTransient<IPaymentServices, PaymentServices>();
            builder.Services.AddTransient<IUserSubscriptionServices, UserSubscriptionServices>();
            builder.Services.AddTransient<ISubscriptionPlanServices, SubscriptionPlanServices>();
            builder.Services.AddTransient<ISubscriptionFeatureServices, SubscriptionFeatureServices>();
            builder.Services.AddTransient<ISubscriptionLimitService, SubscriptionLimitService>();
            builder.Services.AddTransient<IStripeWebhookEventServices, StripeWebhookEventServices>();
            builder.Services.AddTransient<IDashboardService, DashboardService>();

            // Memory Cache per Dashboard
            builder.Services.AddMemoryCache();
            
            // Idealista Service
            builder.Services.AddHttpClient<IIdealistaService, IdealistaService>();
            
            // Access Control Service
            builder.Services.AddScoped<AccessControlService>();

            builder.Services.AddAuthorization(options =>
            {
                // Abbonamento considerato scaduto solo se scaduto da almeno un giorno (il giorno di scadenza è ancora valido)
                options.AddPolicy("ActiveSubscription", policy =>
                    policy.RequireAssertion(context =>
                    {
                        var expiryClaim = context.User.FindFirst("subscription_expiry")?.Value;
                        if (!DateTime.TryParse(expiryClaim, out var expiryDate))
                            return false;
                        var oneDayAfterExpiry = expiryDate.AddDays(1);
                        return oneDayAfterExpiry > DateTime.UtcNow;
                    }));
            });

        }
    }
}