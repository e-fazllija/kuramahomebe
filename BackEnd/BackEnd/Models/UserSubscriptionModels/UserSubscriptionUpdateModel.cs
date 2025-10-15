using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.UserSubscriptionModels
{
    public class UserSubscriptionUpdateModel
    {
        [Required(ErrorMessage = "L'ID è obbligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "L'ID utente è obbligatorio")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "L'ID del piano di abbonamento è obbligatorio")]
        public int SubscriptionPlanId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public bool AutoRenew { get; set; } = true;

        [Required(ErrorMessage = "Lo stato è obbligatorio")]
        [MaxLength(50, ErrorMessage = "Lo stato non può superare i 50 caratteri")]
        public string Status { get; set; } = "active";

        public int? LastPaymentId { get; set; }

        [MaxLength(100, ErrorMessage = "L'ID sottoscrizione Stripe non può superare i 100 caratteri")]
        public string? StripeSubscriptionId { get; set; }

        [MaxLength(100, ErrorMessage = "L'ID cliente Stripe non può superare i 100 caratteri")]
        public string? StripeCustomerId { get; set; }
    }
}
