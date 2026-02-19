using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public enum ConversationType
    {
        Direct = 1,
        Broadcast = 2,
        System = 3
    }

    public class Conversation : EntityBase
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public ConversationType Type { get; set; } = ConversationType.Direct;

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
