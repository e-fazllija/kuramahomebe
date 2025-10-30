using System.Reflection.Emit;
using System.Reflection.Metadata;
using BackEnd.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services
{
    public static class EntitiesConfiguration
    {

        public static ModelBuilder ConfigureEntities(this ModelBuilder builder)
        {
            builder.Entity<RealEstateProperty>()
                .HasOne(c => c.Customer).WithMany(c => c.RealEstateProperties);

            builder.Entity<RealEstateProperty>()
                .HasOne(c => c.Agent).WithMany(c => c.RealEstateProperties).HasForeignKey(p => p.AgentId);

            builder.Entity<RealEstateProperty>()
                .HasMany(c => c.Photos).WithOne(e => e.RealEstateProperty);

            builder.Entity<RealEstateProperty>()
                .HasMany(c => c.RealEstatePropertyNotes).WithOne(e => e.RealEstateProperty).HasForeignKey(e => e.RealEstatePropertyId).OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Request>()
                .HasMany(c => c.RequestNotes);

            builder.Entity<Customer>()
                .HasMany(c => c.CustomerNotes);

            builder.Entity<Customer>()
                .HasOne(c => c.ApplicationUser).WithMany().HasForeignKey(c => c.ApplicationUserId).OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Request>()
                .HasOne(c => c.ApplicationUser).WithMany().HasForeignKey(c => c.ApplicationUserId).OnDelete(DeleteBehavior.NoAction);

            // ==================== INDICI ====================

            // Customer - Indici per ricerche frequenti
            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Customer_Email");

            // Rimosso indice unico su Code (campo eliminato)

            builder.Entity<Customer>()
                .HasIndex(c => new { c.ApplicationUserId, c.CreationDate })
                .HasDatabaseName("IX_Customer_ApplicationUserId_CreationDate");

            // RealEstateProperty - Indici per filtri e ricerche
            builder.Entity<RealEstateProperty>()
                .HasIndex(p => new { p.AgentId, p.CreationDate })
                .HasDatabaseName("IX_RealEstateProperty_AgentId_CreationDate");

            builder.Entity<RealEstateProperty>()
                .HasIndex(p => new { p.City, p.Category, p.Status })
                .HasDatabaseName("IX_RealEstateProperty_City_Category_Status");

            builder.Entity<RealEstateProperty>()
                .HasIndex(p => p.Category)
                .HasDatabaseName("IX_RealEstateProperty_Category");

            builder.Entity<RealEstateProperty>()
                .HasIndex(p => p.Status)
                .HasDatabaseName("IX_RealEstateProperty_Status");

            builder.Entity<RealEstateProperty>()
                .HasIndex(p => p.Sold)
                .HasDatabaseName("IX_RealEstateProperty_Sold");

            builder.Entity<RealEstateProperty>()
                .HasIndex(p => p.Archived)
                .HasDatabaseName("IX_RealEstateProperty_Archived");

            // Calendar - Indici per agenda e filtri temporali
            builder.Entity<Calendar>()
                .HasIndex(c => new { c.ApplicationUserId, c.EventStartDate })
                .HasDatabaseName("IX_Calendar_UserId_StartDate");

            builder.Entity<Calendar>()
                .HasIndex(c => c.EventStartDate)
                .HasDatabaseName("IX_Calendar_EventStartDate");

            builder.Entity<Calendar>()
                .HasIndex(c => new { c.EventStartDate, c.EventEndDate })
                .HasDatabaseName("IX_Calendar_StartDate_EndDate");

            // Request - Indici per ricerche geografiche
            builder.Entity<Request>()
                .HasIndex(r => new { r.Province, r.City })
                .HasDatabaseName("IX_Request_Province_City");

            builder.Entity<Request>()
                .HasIndex(r => r.Archived)
                .HasDatabaseName("IX_Request_Archived");

            builder.Entity<Request>()
                .HasIndex(r => r.Closed)
                .HasDatabaseName("IX_Request_Closed");

            // ApplicationUser - Indici per ricerche agenzie/agenti
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.CreationDate)
                .HasDatabaseName("IX_ApplicationUser_CreationDate");

            // Location - Indice unique per evitare duplicati
            builder.Entity<Location>()
                .HasIndex(l => new { l.Name, l.CityId })
                .IsUnique()
                .HasDatabaseName("IX_Location_Name_CityId");

            // City - Indice unique per evitare duplicati
            builder.Entity<City>()
                .HasIndex(c => new { c.Name, c.ProvinceId })
                .IsUnique()
                .HasDatabaseName("IX_City_Name_ProvinceId");

            // Province - Indice unique sul nome
            builder.Entity<Province>()
                .HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Province_Name");

            // ==================== SUBSCRIPTION SYSTEM ====================

            // UserSubscription relationships
            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserSubscription>()
                .HasOne(us => us.SubscriptionPlan)
                .WithMany(p => p.UserSubscriptions)
                .HasForeignKey(us => us.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment relationships
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany()
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // SubscriptionFeature relationships
            builder.Entity<SubscriptionFeature>()
                .HasOne(f => f.SubscriptionPlan)
                .WithMany(p => p.Features)
                .HasForeignKey(f => f.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription system indexes
            builder.Entity<UserSubscription>()
                .HasIndex(u => u.StripeSubscriptionId)
                .IsUnique(false)
                .HasDatabaseName("IX_UserSubscription_StripeSubscriptionId");

            builder.Entity<Payment>()
                .HasIndex(p => p.StripePaymentIntentId)
                .IsUnique(false)
                .HasDatabaseName("IX_Payment_StripePaymentIntentId");

            builder.Entity<StripeWebhookEvent>()
                .HasIndex(e => e.EventId)
                .IsUnique()
                .HasDatabaseName("IX_StripeWebhookEvent_EventId");

            builder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Active)
                .HasDatabaseName("IX_SubscriptionPlan_Active");

            builder.Entity<UserSubscription>()
                .HasIndex(u => new { u.UserId, u.Status })
                .HasDatabaseName("IX_UserSubscription_UserId_Status");

            return builder;
        }

        #region[Date Localization methods and Params]

        public static DateTime FromLocalToUtc(DateTime date)
        {

            return date.ToUniversalTime();
        }

        public static DateTime? FromLocalToUtcNullable(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToUniversalTime();
            }
            return null;
        }

        #endregion[Date Localization methods and Params]

    }
}
