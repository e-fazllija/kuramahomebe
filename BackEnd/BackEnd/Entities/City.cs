using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Entities
{
    public class City
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int ProvinceId { get; set; }
        
        // Navigation properties
        [ForeignKey("ProvinceId")]
        public virtual Province Province { get; set; } = null!;
        
        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
} 