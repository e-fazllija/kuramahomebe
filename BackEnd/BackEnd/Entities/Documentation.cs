using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class Documentation: EntityBase
    {
        [Required]
        [MaxLength(255, ErrorMessage = "Il nome del file non può superare i 255 caratteri")]
        public string FileName { get; set; } = string.Empty;
        [Required]
        [MaxLength(500, ErrorMessage = "L'URL del file non può superare i 500 caratteri")]
        public string FileUrl { get; set; } = string.Empty;
    }
}
