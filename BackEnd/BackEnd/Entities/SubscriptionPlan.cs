using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class SubscriptionPlan : EntityBase
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, MaxLength(20)]
        public string BillingPeriod { get; set; } = "monthly"; // 'monthly' | 'yearly'

        public bool Active { get; set; } = true;

        [MaxLength(200)]
        public string? StripePriceId { get; set; } // Price ID di Stripe per pagamenti ricorrenti

        // Navigation
        public ICollection<SubscriptionFeature>? Features { get; set; }
        public ICollection<UserSubscription>? UserSubscriptions { get; set; }
    }
}
