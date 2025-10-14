using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Request : EntityBase
    {
        public bool Closed { get; set; }
        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [Required]
        public string Contract { get; set; } = string.Empty;
        [Required]
        public string PropertyType { get; set; } = string.Empty;
        [Required]
        public string province { get; set; } = string.Empty;
        [Required]
        public string Town { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? RoomsNumber { get; set; }
        public int MQFrom { get; set; }
        public int MQTo { get; set; }
        public string? PropertyState { get; set; }
        public string? Heating { get; set; }
        public int ParkingSpaces { get; set; }
        [Required]
        public double PriceTo { get; set; }
        public double PriceFrom { get; set; }
        public int GardenFrom { get; set; }
        public int GardenTo { get; set; }
        public string? Notes { get; set; }
        public bool Archived { get; set; }
        public bool MortgageAdviceRequired { get; set; }
        public ICollection<RequestNotes>? RequestNotes { get; set; }
        public string? AgencyId { get; set; }
        public virtual ApplicationUser? Agency { get; set; }
    }
}
