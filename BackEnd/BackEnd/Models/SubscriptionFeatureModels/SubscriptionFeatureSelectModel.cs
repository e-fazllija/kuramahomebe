namespace BackEnd.Models.SubscriptionFeatureModels
{
    public class SubscriptionFeatureSelectModel
    {
        public int Id { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string FeatureName { get; set; } = null!;
        public string? FeatureValue { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
