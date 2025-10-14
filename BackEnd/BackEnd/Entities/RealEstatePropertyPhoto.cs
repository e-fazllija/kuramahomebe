using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class RealEstatePropertyPhoto : EntityBase
    {
        public int RealEstatePropertyId { get; set; }
        public RealEstateProperty RealEstateProperty { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string Url { get; set; } = string.Empty;
        [Required]
        public int Type { get; set; }
        public bool Highlighted { get; set; }
        [Required]
        public int Position { get; set; }
    }
}
