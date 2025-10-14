using BackEnd.Entities;

namespace BackEnd.Models.AuthModels
{
    public class LoginResponse
    {
        public string Id { get; set; }
        public string AgencyId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Color { get; set; }
    }
}
