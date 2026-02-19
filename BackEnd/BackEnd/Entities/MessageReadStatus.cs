using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class MessageReadStatus
    {
        public int MessageId { get; set; }
        public Message? Message { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
