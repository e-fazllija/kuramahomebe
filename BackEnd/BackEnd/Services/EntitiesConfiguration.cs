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
                .HasOne(c => c.User).WithMany(c => c.RealEstateProperties).HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RealEstateProperty>()
                .HasMany(c => c.Photos).WithOne(e => e.RealEstateProperty);

            builder.Entity<RealEstateProperty>()
                .HasMany(c => c.RealEstatePropertyNotes).WithOne(e => e.RealEstateProperty);

            builder.Entity<Request>()
                .HasMany(c => c.RequestNotes);

            builder.Entity<Customer>()
                .HasMany(c => c.CustomerNotes);
            
            // Configurazione OnDelete per le relazioni uno-a-molti (dalla parte HasOne)
            builder.Entity<RealEstatePropertyPhoto>()
                .HasOne(p => p.RealEstateProperty)
                .WithMany(p => p.Photos)
                .HasForeignKey(p => p.RealEstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequestNotes>()
                .HasOne<Request>()
                .WithMany(r => r.RequestNotes)
                .HasForeignKey(n => n.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerNotes>()
                .HasOne<Customer>()
                .WithMany(c => c.CustomerNotes)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RealEstatePropertyNotes>()
                .HasOne(n => n.RealEstateProperty)
                .WithMany(p => p.RealEstatePropertyNotes)
                .HasForeignKey(n => n.RealEstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Customer>()
                .HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Request>()
                .HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Calendar -> User
            builder.Entity<Calendar>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== INDICI ====================

            // Customer - Indici per ricerche frequenti
            // Rimosso indice unico su Email (ora può essere duplicata)
            // Rimosso indice unico su Code (campo eliminato)

            builder.Entity<Customer>()
                .HasIndex(c => new { c.UserId, c.CreationDate })
                .HasDatabaseName("IX_Customer_UserId_CreationDate");

            // RealEstateProperty - Indici per filtri e ricerche
            builder.Entity<RealEstateProperty>()
                .HasIndex(p => new { p.UserId, p.CreationDate })
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
                .HasIndex(c => new { c.UserId, c.EventStartDate })
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

            // ==================== SUBSCRIPTION SYSTEM ====================

            // UserSubscription relationships
            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade);

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

            // ==================== NOTES RELATIONSHIPS ====================
            // RealEstatePropertyNotes -> User
            builder.Entity<RealEstatePropertyNotes>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RequestNotes -> User
            builder.Entity<RequestNotes>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CustomerNotes -> User
            builder.Entity<CustomerNotes>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== DOCUMENTATION ====================
            // Documentation -> User (UserId) e Agency (AgencyId)
            // Configuriamo le foreign key senza navigation properties per evitare conflitti
            builder.Entity<Documentation>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Documentation_User_UserId");

            builder.Entity<Documentation>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Documentation_Agency_AgencyId");

            // ==================== EXPORT HISTORY ====================
            // ExportHistory -> User
            builder.Entity<ExportHistory>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
