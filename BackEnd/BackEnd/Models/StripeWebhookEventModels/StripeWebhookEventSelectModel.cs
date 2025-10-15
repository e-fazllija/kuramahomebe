namespace BackEnd.Models.StripeWebhookEventModels
{
    public class StripeWebhookEventSelectModel
    {
        public int Id { get; set; }
        public string EventId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Data { get; set; }
        public DateTime ReceivedAt { get; set; }
        public bool Processed { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
