using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Request : EntityBase
    {
        public bool Closed { get; set; }
        [Required(ErrorMessage = "L'ID cliente è obbligatorio")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [Required(ErrorMessage = "Il tipo di contratto è obbligatorio")]
        [MaxLength(50, ErrorMessage = "Il contratto non può superare i 50 caratteri")]
        public string Contract { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il tipo di proprietà è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il tipo di proprietà non può superare i 100 caratteri")]
        public string PropertyType { get; set; } = string.Empty;
        [Required(ErrorMessage = "La provincia è obbligatoria")]
        [MaxLength(100, ErrorMessage = "La provincia non può superare i 100 caratteri")]
        public string Province { get; set; } = string.Empty;
        [Required(ErrorMessage = "La città è obbligatoria")]
        [MaxLength(100, ErrorMessage = "La città non può superare i 100 caratteri")]
        public string City { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "La località non può superare i 100 caratteri")]
        public string? Location { get; set; }
        [MaxLength(50, ErrorMessage = "Il numero di stanze non può superare i 50 caratteri")]
        public string? RoomsNumber { get; set; }
        public int MQFrom { get; set; }
        public int MQTo { get; set; }
        [MaxLength(100, ErrorMessage = "Lo stato della proprietà non può superare i 100 caratteri")]
        public string? PropertyState { get; set; }
        [MaxLength(100, ErrorMessage = "Il riscaldamento non può superare i 100 caratteri")]
        public string? Heating { get; set; }
        public int ParkingSpaces { get; set; }
        [Required(ErrorMessage = "Il prezzo massimo è obbligatorio")]
        public double PriceTo { get; set; }
        public double PriceFrom { get; set; }
        public int GardenFrom { get; set; }
        public int GardenTo { get; set; }
        [MaxLength(2000, ErrorMessage = "Le note non possono superare i 2000 caratteri")]
        public string? Notes { get; set; }
        public bool Archived { get; set; }
        public bool MortgageAdviceRequired { get; set; }
        public ICollection<RequestNotes>? RequestNotes { get; set; }
        [MaxLength(450, ErrorMessage = "L'ID agenzia non può superare i 450 caratteri")]
        public string? AgencyId { get; set; }
        public virtual ApplicationUser? Agency { get; set; }
    }
}
