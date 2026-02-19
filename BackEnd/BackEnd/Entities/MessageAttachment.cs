using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class MessageAttachment : EntityBase
    {
        public int MessageId { get; set; }
        public Message? Message { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;
    }
}
