using BackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RequestModels
{
    public class RequestCreateModel
    {
        public bool Closed { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public string Contract { get; set; } = string.Empty;
        [Required]
        public string PropertyType { get; set; } = string.Empty;
        [Required]
        public string Province { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        public int RoomsFrom { get; set; }
        public int RoomsTo { get; set; }
        public int Bathrooms { get; set; }
        public string? Floor { get; set; }
        public int MQFrom { get; set; }
        public int MQTo { get; set; }
        public string? PropertyState { get; set; }
        public string? Heating { get; set; }
        public string? Furniture { get; set; }
        public string? EnergyClass { get; set; }
        public bool Auction { get; set; }
        public int ParkingSpaces { get; set; }
        [Required]
        public double PriceTo { get; set; }
        public double PriceFrom { get; set; }
        public int GardenFrom { get; set; }
        public int GardenTo { get; set; }
        public string? Notes { get; set; }
        public bool Archived { get; set; }
        public bool MortgageAdviceRequired { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
    }
}
