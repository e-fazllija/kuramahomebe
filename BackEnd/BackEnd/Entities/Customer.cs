using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Customer : EntityBase
    {
        // Code rimosso: utilizziamo l'Id ereditato da EntityBase
        public bool Buyer { get; set; }
        public bool Seller { get; set; }
        public bool Builder { get; set; }
        public bool Other { get; set; }
        public bool GoldCustomer { get; set; }
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il cognome non può superare i 100 caratteri")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [MaxLength(256, ErrorMessage = "L'email non può superare i 256 caratteri")]
        [EmailAddress(ErrorMessage = "Formato email non valido")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il telefono è obbligatorio")]
        public long Phone { get; set; } 
        [MaxLength(1000, ErrorMessage = "La descrizione non può superare i 1000 caratteri")]
        public string? Description { get; set; } 
        [MaxLength(300, ErrorMessage = "L'indirizzo non può superare i 300 caratteri")]
        public string? Address { get; set; } 
        [MaxLength(100, ErrorMessage = "La città non può superare i 100 caratteri")]
        public string? City { get; set; } 
        [MaxLength(100, ErrorMessage = "Lo stato non può superare i 100 caratteri")]
        public string? State { get; set; } 
        public bool AcquisitionDone { get; set; }
        public bool OngoingAssignment { get; set; }
        public virtual ICollection<RealEstateProperty>? RealEstateProperties { get; set; }
        public virtual ICollection<CustomerNotes>? CustomerNotes { get; set; }
        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        [Required(ErrorMessage = "L'AdminId è obbligatorio")]
        [MaxLength(450, ErrorMessage = "L'AdminId non può superare i 450 caratteri")]
        public string AdminId { get; set; } = string.Empty;
    }
}
