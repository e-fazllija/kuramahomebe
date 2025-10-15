using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class RealEstatePropertyNotes: EntityBase
    {
        [Required]
        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int? CalendarId { get; set; }
        public int RealEstatePropertyId { get; set; }
        public RealEstateProperty RealEstateProperty { get; set; }
        [Required]
        [MaxLength(2000, ErrorMessage = "Il testo della nota non può superare i 2000 caratteri")]
        public string Text { get; set; } = string.Empty;
    }
}
