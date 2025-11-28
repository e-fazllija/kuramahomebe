using BackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.UserModel
{
    public class UserSelectModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? Referent { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public string? Province { get; set; }
        public string? Role { get; set; }
        public string? AdminId { get; set; }
        public string Color { get; set; } = "#ffffff";
        public bool EmailConfirmed { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }

        // --- DATI FISCALI / FATTURAZIONE ---
        public UserType UserType { get; set; }
        public string? FiscalCode { get; set; }
        public string? CompanyName { get; set; }
        public string? VATNumber { get; set; }
        public string? PEC { get; set; }
        public string? SDICode { get; set; }

        // --- CONFIGURAZIONE IDEALISTA ---
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public bool? SyncToIdealista { get; set; }
    }
}
