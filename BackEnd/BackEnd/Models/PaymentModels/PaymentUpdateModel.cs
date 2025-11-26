using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.PaymentModels
{
    public class PaymentUpdateModel
    {
        [Required(ErrorMessage = "L'ID è obbligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "L'ID utente è obbligatorio")]
        public string UserId { get; set; } = null!;

        public int? SubscriptionId { get; set; }

        [Required(ErrorMessage = "L'importo è obbligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "L'importo deve essere maggiore o uguale a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La valuta è obbligatoria")]
        [MaxLength(10, ErrorMessage = "La valuta non può superare i 10 caratteri")]
        public string Currency { get; set; } = "EUR";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Il metodo di pagamento è obbligatorio")]
        [MaxLength(50, ErrorMessage = "Il metodo di pagamento non può superare i 50 caratteri")]
        public string PaymentMethod { get; set; } = "stripe";

        [MaxLength(255, ErrorMessage = "L'ID transazione non può superare i 255 caratteri")]
        public string? TransactionId { get; set; }

        [Required(ErrorMessage = "Lo stato è obbligatorio")]
        [MaxLength(50, ErrorMessage = "Lo stato non può superare i 50 caratteri")]
        public string Status { get; set; } = "pending";

        [MaxLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri")]
        public string? Notes { get; set; }

        [MaxLength(100, ErrorMessage = "L'ID PaymentIntent Stripe non può superare i 100 caratteri")]
        public string? StripePaymentIntentId { get; set; }

        [MaxLength(100, ErrorMessage = "L'ID Charge Stripe non può superare i 100 caratteri")]
        public string? StripeChargeId { get; set; }
    }
}
