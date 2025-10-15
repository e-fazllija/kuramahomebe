using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class UserSubscription : EntityBase
    {
        [Required]
        public string UserId { get; set; } = null!; // FK verso AspNetUsers

        [Required]
        public int SubscriptionPlanId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public bool AutoRenew { get; set; } = true;

        [Required, MaxLength(50)]
        public string Status { get; set; } = "active"; // active | expired | cancelled

        public int? LastPaymentId { get; set; }

        // Stripe
        [MaxLength(100)]
        public string? StripeSubscriptionId { get; set; }

        [MaxLength(100)]
        public string? StripeCustomerId { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public Payment? LastPayment { get; set; }
    }
}
