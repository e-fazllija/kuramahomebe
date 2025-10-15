using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Entities
{
    public class Location : EntityBase
    {
        [Required(ErrorMessage = "Il nome della località è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'ID città è obbligatorio")]
        public int CityId { get; set; }
        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string? ApplicationUserId { get; set; }
        
        // Navigation property
        [ForeignKey("CityId")]
        public virtual City City { get; set; } = null!;
    }
} 