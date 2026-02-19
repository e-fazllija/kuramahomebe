using BackEnd.Models.ChatModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IConversationServices
    {
        Task<List<ConversationDto>> GetConversationsForUser(string userId);
        Task<ConversationDto> GetConversationById(int id, string userId);
        Task<ConversationDto> CreateConversation(string creatorId, CreateConversationDto dto);
        Task<List<int>> GetConversationIdsForUser(string userId);
        Task<MessageDto> SendMessage(string senderId, SendMessageDto dto);
        Task MarkAsRead(string userId, MarkReadDto dto);
        Task<List<MessageDto>> GetMessages(int conversationId, string userId, int page, int pageSize);
        Task<List<ContactDto>> GetContacts(string userId);
        Task<List<ConversationDto>> GetMonitoringConversations(string supervisorId);
        Task<int> GetTotalUnreadCount(string userId);
    }
}
