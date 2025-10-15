using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.AuthModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Lastname is required")]
        public string LastName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? Referent { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? AgencyId { get; set; }
        public string Color { get; set; } = "#ffffff";
    }
}
