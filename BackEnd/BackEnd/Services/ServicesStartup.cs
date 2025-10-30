using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Services.BusinessServices;

namespace BackEnd.Services
{
    public static class ServicesStartup
    {

      
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {

            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IStorageServices, StorageServices>();
            builder.Services.AddTransient<IMailService, MailService>();
            builder.Services.AddTransient<IGenericService, GenericService>();
            builder.Services.AddTransient<ICustomerServices, CustomerServices>();
            builder.Services.AddTransient<IRealEstatePropertyServices, RealEstatePropertyServices>();
            builder.Services.AddTransient<IRealEstatePropertyPhotoServices, RealEstatePropertyPhotoServices>();
            builder.Services.AddTransient<IRequestServices, RequestServices>();
            builder.Services.AddTransient<ICalendarServices, CalendarServices>();
            builder.Services.AddTransient<IDocumentsTabServices, DocumentsTabServices>();
            builder.Services.AddTransient<ILocationServices, LocationServices>();
            
            // Billing & Subscription Services
            builder.Services.AddTransient<IStripeService, StripeService>();
            builder.Services.AddTransient<IPaymentServices, PaymentServices>();
            builder.Services.AddTransient<IUserSubscriptionServices, UserSubscriptionServices>();
            builder.Services.AddTransient<ISubscriptionPlanServices, SubscriptionPlanServices>();
            builder.Services.AddTransient<ISubscriptionFeatureServices, SubscriptionFeatureServices>();
            builder.Services.AddTransient<ISubscriptionLimitService, SubscriptionLimitService>();
            builder.Services.AddTransient<IStripeWebhookEventServices, StripeWebhookEventServices>();

            builder.Services.AddTransient<ProvinceServices, ProvinceServices>();
            builder.Services.AddTransient<CityServices, CityServices>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ActiveSubscription", policy =>
                    policy.RequireAssertion(context =>
                    {
                        var expiryClaim = context.User.FindFirst("subscription_expiry")?.Value;
                        return DateTime.TryParse(expiryClaim, out var expiryDate) && expiryDate > DateTime.UtcNow;
                    }));
            });

        }
    }
}