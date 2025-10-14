using BackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CustomerModels
{
    public class CustomerCreateModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        public bool Buyer { get; set; }
        public bool Seller { get; set; }
        public bool Builder { get; set; }
        public bool Other { get; set; }
        public bool GoldCustomer { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Required]
        public long Phone { get; set; }
        public string? Description { get; set; }
        public string? AdressLine { get; set; }
        public string? Town { get; set; }
        public string? State { get; set; }
        public bool AcquisitionDone { get; set; }
        public bool OngoingAssignment { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string AgencyId { get; set; } = string.Empty;
    }
}
