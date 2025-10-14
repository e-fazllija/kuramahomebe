using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CityModels
{
    public class CityCreateModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int ProvinceId { get; set; }
    }
} 