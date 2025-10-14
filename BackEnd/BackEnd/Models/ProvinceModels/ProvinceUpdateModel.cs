using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.ProvinceModels
{
    public class ProvinceUpdateModel
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
} 