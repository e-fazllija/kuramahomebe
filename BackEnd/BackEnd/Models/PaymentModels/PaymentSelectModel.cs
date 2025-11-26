namespace BackEnd.Models.PaymentModels
{
    public class PaymentSelectModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int? SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? StripeChargeId { get; set; }
    }
}
