using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.LocationModels
{
    public class LocationCreateModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int CityId { get; set; }
    }
} 