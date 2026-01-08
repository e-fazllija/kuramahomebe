using System.ComponentModel.DataAnnotations;
using BackEnd.Entities;

namespace BackEnd.Models.UserModel
{
    public class UserCreateModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Lastname is required")]
        public string LastName { get; set; }
        
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        
        public string? UserName { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string? MobilePhone { get; set; }
        
        public string? Referent { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        
        public string? Province { get; set; }
        
        public string? ZipCode { get; set; }
        
        public string? Country { get; set; } = "Italia";
        
        public string Color { get; set; } = "#ffffff";

        // --- AGENZIA ---
        public string? AgencyId { get; set; }

        // --- TIPO UTENTE ---
        [Required(ErrorMessage = "User type is required")]
        public UserType UserType { get; set; }

        // --- DATI FISCALI ---
        public string? CompanyName { get; set; }
        public string? FiscalCode { get; set; }
        public string? VATNumber { get; set; }
        public string? PEC { get; set; }
        public string? SDICode { get; set; }

        // --- ABBONAMENTO (per agenzie) ---
        /// <summary>
        /// ID del piano di abbonamento da assegnare all'agenzia durante la creazione.
        /// Se specificato, verrà creato un abbonamento attivo per l'agenzia.
        /// </summary>
        public int? SubscriptionPlanId { get; set; }
    }
}
