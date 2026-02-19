namespace BackEnd.Models.ChatModels
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int Type { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public List<ParticipantDto> Participants { get; set; } = new();
    }

    public class ParticipantDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Color { get; set; } = "#ffffff";
        public bool IsMonitor { get; set; }
    }

    public class CreateConversationDto
    {
        public string? Title { get; set; }
        public int Type { get; set; } = 1;
        public List<string> ParticipantIds { get; set; } = new();
        public string? InitialMessage { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderFirstName { get; set; } = string.Empty;
        public string SenderLastName { get; set; } = string.Empty;
        public string SenderColor { get; set; } = "#ffffff";
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationDate { get; set; }
        public List<MessageAttachmentDto> Attachments { get; set; } = new();
        public List<string> ReadByUserIds { get; set; } = new();
    }

    public class SendMessageDto
    {
        public int ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; } = 1;
    }

    public class MessageAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class MarkReadDto
    {
        public int ConversationId { get; set; }
        public int LastMessageId { get; set; }
    }

    public class ContactDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Color { get; set; } = "#ffffff";
        public string? CompanyName { get; set; }
    }

    public class MonitoringConversationDto : ConversationDto
    {
        public string Participant1Name { get; set; } = string.Empty;
        public string Participant2Name { get; set; } = string.Empty;
    }
}
