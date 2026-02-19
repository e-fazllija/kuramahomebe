using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public enum MessageType
    {
        Text = 1,
        System = 2,
        Broadcast = 3
    }

    public class Message : EntityBase
    {
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        [Required]
        [MaxLength(450)]
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;
        public bool IsDeleted { get; set; } = false;

        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public ICollection<MessageReadStatus> ReadStatuses { get; set; } = new List<MessageReadStatus>();
    }
}
