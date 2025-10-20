using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public enum UserType
    {
        PersonaFisica = 1,
        PersonaGiuridica = 2
    }

    public class ApplicationUser : IdentityUser
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

        [MaxLength(10, ErrorMessage = "Il CAP non può superare i 10 caratteri")]
        public string? ZipCode { get; set; }

        [MaxLength(100, ErrorMessage = "La nazione non può superare i 100 caratteri")]
        public string? Country { get; set; } = "Italia";

        [MaxLength(450, ErrorMessage = "L'ID agenzia non può superare i 450 caratteri")]
        public string? AgencyId { get; set; }

        [MaxLength(20, ErrorMessage = "Il colore non può superare i 20 caratteri")]
        public string Color { get; set; } = "#ffffff";

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        public ApplicationUser? Agency { get; set; }

        // --- DATI FISCALI / FATTURAZIONE ---

        [Required(ErrorMessage = "Specificare se persona fisica o giuridica")]
        public UserType UserType { get; set; }

        [MaxLength(150, ErrorMessage = "La ragione sociale non può superare i 150 caratteri")]
        public string? CompanyName { get; set; }

        [MaxLength(16, ErrorMessage = "Il codice fiscale non può superare i 16 caratteri")]
        public string? FiscalCode { get; set; }

        [MaxLength(11, ErrorMessage = "La partita IVA deve avere 11 caratteri")]
        public string? VATNumber { get; set; }

        [EmailAddress(ErrorMessage = "Formato PEC non valido")]
        [MaxLength(150, ErrorMessage = "La PEC non può superare i 150 caratteri")]
        public string? PEC { get; set; }

        [MaxLength(7, ErrorMessage = "Il codice destinatario non può superare i 7 caratteri")]
        public string? SDICode { get; set; }

        // --- RELAZIONI ---
        public ICollection<RealEstateProperty>? RealEstateProperties { get; set; }
        public ICollection<UserSubscription>? Subscriptions { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        // public ICollection<Calendar>? Calendars { get; set; }
    }
}
