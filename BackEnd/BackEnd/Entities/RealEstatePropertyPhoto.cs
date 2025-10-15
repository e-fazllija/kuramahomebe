using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class RealEstatePropertyPhoto : EntityBase
    {
        public int RealEstatePropertyId { get; set; }
        public RealEstateProperty RealEstateProperty { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Il nome del file non può superare i 255 caratteri")]
        public string FileName { get; set; } = string.Empty;
        [Required]
        [MaxLength(500, ErrorMessage = "L'URL non può superare i 500 caratteri")]
        public string Url { get; set; } = string.Empty;
        [Required]
        public int Type { get; set; }
        public bool Highlighted { get; set; }
        [Required]
        public int Position { get; set; }
    }
}
