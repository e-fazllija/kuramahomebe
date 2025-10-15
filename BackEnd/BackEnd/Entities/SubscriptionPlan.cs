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

        public int? MaxUsers { get; set; }
        public int? MaxProperties { get; set; }

        public bool Active { get; set; } = true;

        // Navigation
        public ICollection<SubscriptionFeature>? Features { get; set; }
        public ICollection<UserSubscription>? UserSubscriptions { get; set; }
    }
}
