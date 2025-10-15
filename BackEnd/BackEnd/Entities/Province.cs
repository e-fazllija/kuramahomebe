using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Province
    {
        public int Id { get; set; }

        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string? ApplicationUserId { get; set; }
        
        [Required(ErrorMessage = "Il nome della provincia è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
} 