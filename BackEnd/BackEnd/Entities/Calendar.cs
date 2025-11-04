using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Calendar : EntityBase
    {
        [Required]
        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; }
        [Required]
        [MaxLength(200, ErrorMessage = "Il nome dell'evento non può superare i 200 caratteri")]
        public string EventName { get; set; } = string.Empty;
        [Required]
        [MaxLength(50, ErrorMessage = "Il tipo di evento non può superare i 50 caratteri")]
        public string Type { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? RealEstatePropertyId { get; set; }
        public RealEstateProperty? RealEstateProperty { get; set; }
        public int? RequestId { get; set; }
        public Request? Request { get; set; }
        [MaxLength(1000, ErrorMessage = "La descrizione dell'evento non può superare i 1000 caratteri")]
        public string? EventDescription { get; set; }
        [MaxLength(300, ErrorMessage = "La posizione dell'evento non può superare i 300 caratteri")]
        public string? EventLocation { get; set; }
        [MaxLength(20, ErrorMessage = "Il colore non può superare i 20 caratteri")]
        public string? Color { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public bool Confirmed { get; set; }
        public bool Cancelled { get; set; }
        public bool Postponed { get; set; }
    }
}
