using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.SubscriptionFeatureModels
{
    public class SubscriptionFeatureUpdateModel
    {
        [Required(ErrorMessage = "L'ID è obbligatorio")]
        public int Id { get; set; }

        [Required(ErrorMessage = "L'ID del piano di abbonamento è obbligatorio")]
        public int SubscriptionPlanId { get; set; }

        [Required(ErrorMessage = "Il nome della funzionalità è obbligatorio")]
        [MaxLength(200, ErrorMessage = "Il nome della funzionalità non può superare i 200 caratteri")]
        public string FeatureName { get; set; } = null!;
        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "Il valore della funzionalità non può superare i 200 caratteri")]
        public string? FeatureValue { get; set; }
    }
}
