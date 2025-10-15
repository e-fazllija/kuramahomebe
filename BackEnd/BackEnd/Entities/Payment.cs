using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Payment : EntityBase
    {
        [Required]
        public string UserId { get; set; } = null!;

        public int? SubscriptionId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "EUR";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "stripe"; // stripe | paypal | bank_transfer

        [MaxLength(255)]
        public string? TransactionId { get; set; } // Stripe PaymentIntentId

        [Required, MaxLength(50)]
        public string Status { get; set; } = "pending"; // pending | completed | failed | refunded

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Stripe fields
        [MaxLength(100)]
        public string? StripePaymentIntentId { get; set; }

        [MaxLength(100)]
        public string? StripeChargeId { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public UserSubscription? Subscription { get; set; }
    }
}
