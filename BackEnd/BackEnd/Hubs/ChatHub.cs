using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ChatModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackEnd.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IConversationServices _conversationServices;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IConversationServices conversationServices, ILogger<ChatHub> logger)
        {
            _conversationServices = conversationServices;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Context.Abort();
                return;
            }

            // Join personal group to receive targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Join all conversation groups
            var convIds = await _conversationServices.GetConversationIdsForUser(userId);
            foreach (var convId in convIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{convId}");
            }

            _logger.LogInformation("Utente {UserId} connesso alla chat hub", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Utente {UserId} disconnesso dalla chat hub", userId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client sends a message to a conversation.
        /// </summary>
        public async Task SendMessage(SendMessageDto dto)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var message = await _conversationServices.SendMessage(userId, dto);

                // Broadcast to all conversation participants
                await Clients.Group($"conv_{dto.ConversationId}").SendAsync("ReceiveMessage", message);

                // Update unread count for non-sender participants
                await Clients.Group($"conv_{dto.ConversationId}").SendAsync("ConversationUpdated", new
                {
                    ConversationId = dto.ConversationId,
                    LastMessage = message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'invio del messaggio");
                await Clients.Caller.SendAsync("Error", "Si è verificato un errore nell'invio del messaggio");
            }
        }

        /// <summary>
        /// Client marks messages in a conversation as read.
        /// </summary>
        public async Task MarkRead(MarkReadDto dto)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                await _conversationServices.MarkAsRead(userId, dto);

                // Notify other participants that this user has read the messages
                await Clients.Group($"conv_{dto.ConversationId}").SendAsync("MessagesRead", new
                {
                    UserId = userId,
                    dto.ConversationId,
                    dto.LastMessageId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel marcare i messaggi come letti");
            }
        }

        /// <summary>
        /// Joins a new conversation group (called after creating a conversation).
        /// </summary>
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        }
    }
}
