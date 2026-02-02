using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.SubscriptionPlanModels
{
    public class SubscriptionPlanCreateModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "La descrizione non può superare i 500 caratteri")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "Il prezzo deve essere maggiore o uguale a 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Il periodo di fatturazione è obbligatorio")]
        [MaxLength(20, ErrorMessage = "Il periodo di fatturazione non può superare i 20 caratteri")]
        public string BillingPeriod { get; set; } = "monthly";

        public bool Active { get; set; } = true;

        [MaxLength(200, ErrorMessage = "Lo Stripe Price ID non può superare i 200 caratteri")]
        public string? StripePriceId { get; set; }
    }
}
