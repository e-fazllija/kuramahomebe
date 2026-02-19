using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class ConversationParticipant : EntityBase
    {
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int? LastReadMessageId { get; set; }
        public DateTime? LastReadAt { get; set; }

        /// <summary>
        /// True when Admin/Agency are monitoring a conversation without being direct participants.
        /// </summary>
        public bool IsMonitor { get; set; } = false;
    }
}
