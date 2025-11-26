using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class RequestNotes : EntityBase
    {
        [Required]
        [MaxLength(450, ErrorMessage = "L'ID utente non può superare i 450 caratteri")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int? CalendarId { get; set; }
        public int RequestId { get; set; }
        [Required]
        [MaxLength(2000, ErrorMessage = "Il testo della nota non può superare i 2000 caratteri")]
        public string Text { get; set; } = string.Empty;
    }
}
