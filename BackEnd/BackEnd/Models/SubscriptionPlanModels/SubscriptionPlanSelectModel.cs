using BackEnd.Models.SubscriptionFeatureModels;

namespace BackEnd.Models.SubscriptionPlanModels
{
    public class SubscriptionPlanSelectModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string BillingPeriod { get; set; } = null!;
        public bool Active { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<SubscriptionFeatureSelectModel>? Features { get; set; }
    }
}
