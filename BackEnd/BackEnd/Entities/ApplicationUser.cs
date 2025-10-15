using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il cognome non può superare i 100 caratteri")]
        public string LastName { get; set; } = string.Empty;
        [MaxLength(50, ErrorMessage = "Il numero di cellulare non può superare i 50 caratteri")]
        public string? MobilePhone { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "Il referente non può superare i 100 caratteri")]
        public string? Referent { get; set; } = string.Empty;
        [Required(ErrorMessage = "L'indirizzo è obbligatorio")]
        [MaxLength(300, ErrorMessage = "L'indirizzo non può superare i 300 caratteri")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "La città è obbligatoria")]
        [MaxLength(100, ErrorMessage = "La città non può superare i 100 caratteri")]
        public string City { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "La regione non può superare i 100 caratteri")]
        public string? Region { get; set; }
        [MaxLength(450, ErrorMessage = "L'ID agenzia non può superare i 450 caratteri")]
        public string? AgencyId { get; set; }
        [MaxLength(20, ErrorMessage = "Il colore non può superare i 20 caratteri")]
        public string Color { get; set; } = "#ffffff";
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        public ApplicationUser? Agency { get; set; }
        public ICollection<RealEstateProperty>? RealEstateProperties { get; set; }
        public ICollection<UserSubscription>? Subscriptions { get; set; }
        public ICollection<Payment>? Payments { get; set; }
       //public ICollection<Calendar>? Calendars { get; set; }
    }
}
