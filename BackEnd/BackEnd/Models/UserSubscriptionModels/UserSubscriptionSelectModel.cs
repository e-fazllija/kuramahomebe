using BackEnd.Models.SubscriptionPlanModels;
using BackEnd.Models.PaymentModels;

namespace BackEnd.Models.UserSubscriptionModels
{
    public class UserSubscriptionSelectModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int SubscriptionPlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AutoRenew { get; set; }
        public string Status { get; set; } = null!;
        public int? LastPaymentId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public SubscriptionPlanSelectModel? SubscriptionPlan { get; set; }
        public PaymentSelectModel? LastPayment { get; set; }
    }
}
