using BackEnd.Entities;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.RealEstatePropertyModels;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CalendarModels
{
    public class CalendarSelectModel
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = new ApplicationUser();
        public string EventName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public virtual CustomerSelectModel? Customer { get; set; }
        public int? RealEstatePropertyId { get; set; }
        public virtual RealEstatePropertySelectModel? RealEstateProperty { get; set; }
        public int? RequestId { get; set; }
        public virtual Request? Request { get; set; }
        public string? EventDescription { get; set; }
        public string? EventLocation { get; set; }
        public string? Color { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Confirmed { get; set; }
        public bool Cancelled { get; set; }
        public bool Postponed { get; set; }
    }
}
