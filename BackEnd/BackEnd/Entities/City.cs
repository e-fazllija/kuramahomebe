using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Entities
{
    public class City
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Il nome della città è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string? UserId { get; set; }
        
        [Required(ErrorMessage = "L'ID provincia è obbligatorio")]
        public int ProvinceId { get; set; }
        
        // Navigation properties
        [ForeignKey("ProvinceId")]
        public virtual Province Province { get; set; } = null!;
        
        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}