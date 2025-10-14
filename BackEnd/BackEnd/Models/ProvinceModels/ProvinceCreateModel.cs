using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.ProvinceModels
{
    public class ProvinceCreateModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
} 