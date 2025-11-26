using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class SubscriptionFeature : EntityBase
    {
        public int SubscriptionPlanId { get; set; }

        [Required, MaxLength(200)]
        public string FeatureName { get; set; } = null!;

        [MaxLength(200)]
        public string? FeatureValue { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation
        public SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}
