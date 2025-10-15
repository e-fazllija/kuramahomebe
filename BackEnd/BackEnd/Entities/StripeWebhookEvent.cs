using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class StripeWebhookEvent : EntityBase
    {
        [Required, MaxLength(100)]
        public string EventId { get; set; } = null!; // ID univoco di Stripe

        [Required, MaxLength(100)]
        public string Type { get; set; } = null!;

        [MaxLength(10000, ErrorMessage = "I dati JSON non possono superare i 10000 caratteri")]
        public string? Data { get; set; } // JSON payload completo

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public bool Processed { get; set; } = false;
    }
}
