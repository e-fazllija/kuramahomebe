using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Entities
{
    public class Location : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int CityId { get; set; }
        
        // Navigation property
        [ForeignKey("CityId")]
        public virtual City City { get; set; } = null!;
    }
} 