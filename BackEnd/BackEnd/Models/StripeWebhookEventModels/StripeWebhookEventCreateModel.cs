using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.StripeWebhookEventModels
{
    public class StripeWebhookEventCreateModel
    {
        [Required(ErrorMessage = "L'ID evento è obbligatorio")]
        [MaxLength(100, ErrorMessage = "L'ID evento non può superare i 100 caratteri")]
        public string EventId { get; set; } = null!;

        [Required(ErrorMessage = "Il tipo evento è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il tipo evento non può superare i 100 caratteri")]
        public string Type { get; set; } = null!;

        public string? Data { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public bool Processed { get; set; } = false;
    }
}
