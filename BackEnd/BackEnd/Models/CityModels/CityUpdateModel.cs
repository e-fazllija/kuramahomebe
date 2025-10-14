using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CityModels
{
    public class CityUpdateModel
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int ProvinceId { get; set; }
    }
} 