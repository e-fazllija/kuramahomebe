using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public string? MobilePhone { get; set; } = string.Empty;
        public string? Referent { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string Town { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? AgencyId { get; set; }
        public string Color { get; set; } = "#ffffff";
        public ApplicationUser? Agency { get; set; }
        public ICollection<RealEstateProperty>? RealEstateProperties { get; set; }
       //public ICollection<Calendar>? Calendars { get; set; }
    }
}
