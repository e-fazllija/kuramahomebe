using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    /// <summary>
    /// Entità per tracciare gli export effettuati dagli utenti (per limiti mensili)
    /// </summary>
    public class ExportHistory : EntityBase
    {
        [Required]
        public string UserId { get; set; } = null!; // Admin root che ha effettuato l'export

        [Required, MaxLength(50)]
        public string ExportType { get; set; } = null!; // "excel", "csv"

        [MaxLength(200)]
        public string? EntityType { get; set; } // "requests", "customers", "properties", etc.

        public DateTime ExportDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}


