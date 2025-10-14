using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.UserModel
{
    public class UserSelectModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? Referent { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? Role { get; set; }
        public string? AgencyId { get; set; }
        public string Color { get; set; } = "#ffffff";
        public bool EmailConfirmed { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
