using BackEnd.Entities;
using BackEnd.Models.RealEstatePropertyModels;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RequestModels
{
    public class RequestSelectModel
    {
        public int Id { get; set; }
        public bool Closed { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public string Contract { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? RoomsNumber { get; set; }
        public int MQFrom { get; set; }
        public int MQTo { get; set; }
        public string? PropertyState { get; set; }
        public string? Heating { get; set; }
        public int ParkingSpaces { get; set; }
        public double PriceTo { get; set; }
        public double PriceFrom { get; set; }
        public int GardenFrom { get; set; }
        public int GardenTo { get; set; }
        public string? Notes { get; set; }
        public bool Archived { get; set; }
        public bool MortgageAdviceRequired { get; set; }
        public List<RealEstatePropertySelectModel>? RealEstateProperties { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public ICollection<RequestNotes>? RequestNotes { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
