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
        
        // Nuovi campi per la gestione cartelle e privacy
        [MaxLength(450)]
        public string? AgencyId { get; set; }
        
        [MaxLength(450)]
        public string? UserId { get; set; }
        
        [Required]
        public bool IsFolder { get; set; } = false;
        
        [Required]
        public bool IsPrivate { get; set; } = false;
        
        [MaxLength(1000)]
        public string? ParentPath { get; set; }
        
        [MaxLength(255)]
        public string? DisplayName { get; set; }
    }
}
