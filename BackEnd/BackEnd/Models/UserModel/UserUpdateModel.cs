using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.UserModel
{
    public class UserUpdateModel
    {
        [Required(ErrorMessage = "Id is required")]
        public string Id { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Lastname is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? Referent { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public string? Region { get; set; }
        public string Color { get; set; } = "#ffffff";
        public string AgencyId { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        // --- DATI FISCALI / FATTURAZIONE ---
        // Nota: UserType non è modificabile dopo la registrazione, quindi non è incluso nell'update
        public string? FiscalCode { get; set; }
        public string? CompanyName { get; set; }
        public string? VATNumber { get; set; }
        public string? PEC { get; set; }
        public string? SDICode { get; set; }
    }
}
