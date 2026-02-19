using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ChatModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.BusinessServices
{
    public class ConversationServices : IConversationServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;
        private readonly ILogger<ConversationServices> _logger;

        public ConversationServices(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControl,
            ILogger<ConversationServices> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _accessControl = accessControl;
            _logger = logger;
        }

        public async Task<List<ConversationDto>> GetConversationsForUser(string userId)
        {
            try
            {
                var participations = await _unitOfWork.dbContext.ConversationParticipants
                    .Where(p => p.UserId == userId)
                    .Select(p => p.ConversationId)
                    .ToListAsync();

                var conversations = await _unitOfWork.dbContext.Conversations
                    .Where(c => participations.Contains(c.Id))
                    .Include(c => c.Participants).ThenInclude(p => p.User)
                    .Include(c => c.Messages.OrderByDescending(m => m.CreationDate).Take(1))
                        .ThenInclude(m => m.Attachments)
                    .OrderByDescending(c => c.UpdateDate)
                    .ToListAsync();

                var result = new List<ConversationDto>();
                foreach (var conv in conversations)
                {
                    var dto = await MapConversationToDto(conv, userId);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero delle conversazioni per l'utente {UserId}", userId);
                throw new Exception("Si è verificato un errore nel recupero delle conversazioni");
            }
        }

        public async Task<ConversationDto> GetConversationById(int id, string userId)
        {
            try
            {
                var conv = await _unitOfWork.dbContext.Conversations
                    .Include(c => c.Participants).ThenInclude(p => p.User)
                    .Include(c => c.Messages.OrderByDescending(m => m.CreationDate).Take(1))
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (conv == null)
                    throw new Exception("Conversazione non trovata");

                var participant = conv.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                    throw new UnauthorizedAccessException("Non hai accesso a questa conversazione");

                return await MapConversationToDto(conv, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero della conversazione {ConvId}", id);
                throw;
            }
        }

        public async Task<ConversationDto> CreateConversation(string creatorId, CreateConversationDto dto)
        {
            try
            {
                var convType = (ConversationType)dto.Type;

                // For direct conversations, check if one already exists between these two users
                if (convType == ConversationType.Direct && dto.ParticipantIds.Count == 1)
                {
                    var otherUserId = dto.ParticipantIds[0];
                    var existing = await FindDirectConversation(creatorId, otherUserId);
                    if (existing != null)
                        return await MapConversationToDto(existing, creatorId);
                }

                var now = DateTime.UtcNow;
                var conversation = new Conversation
                {
                    Title = dto.Title,
                    Type = convType,
                    CreatedByUserId = creatorId,
                    CreationDate = now,
                    UpdateDate = now
                };

                await _unitOfWork.ConversationRepository.InsertAsync(conversation);
                await _unitOfWork.SaveAsync();

                // Add creator as participant
                var creatorParticipant = new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    UserId = creatorId,
                    CreationDate = now,
                    UpdateDate = now
                };
                await _unitOfWork.dbContext.ConversationParticipants.AddAsync(creatorParticipant);

                // Add other participants
                foreach (var participantId in dto.ParticipantIds)
                {
                    if (participantId == creatorId) continue;
                    var participant = new ConversationParticipant
                    {
                        ConversationId = conversation.Id,
                        UserId = participantId,
                        CreationDate = now,
                        UpdateDate = now
                    };
                    await _unitOfWork.dbContext.ConversationParticipants.AddAsync(participant);
                }

                // For Broadcast/System, add admin/agency supervisors as monitors
                if (convType == ConversationType.Broadcast || convType == ConversationType.System)
                {
                    await AddMonitorsToConversation(conversation.Id, creatorId, now, dto.ParticipantIds);
                }

                await _unitOfWork.SaveAsync();

                // Send initial message if provided
                if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
                {
                    var msgType = convType == ConversationType.Broadcast ? MessageType.Broadcast
                        : convType == ConversationType.System ? MessageType.System
                        : MessageType.Text;

                    var message = new Message
                    {
                        ConversationId = conversation.Id,
                        SenderId = creatorId,
                        Content = dto.InitialMessage,
                        Type = msgType,
                        CreationDate = now,
                        UpdateDate = now
                    };
                    await _unitOfWork.dbContext.Messages.AddAsync(message);
                    await _unitOfWork.SaveAsync();

                    conversation.UpdateDate = now;
                    _unitOfWork.dbContext.Conversations.Update(conversation);
                    await _unitOfWork.SaveAsync();
                }

                return await GetConversationById(conversation.Id, creatorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione della conversazione");
                throw new Exception("Si è verificato un errore nella creazione della conversazione");
            }
        }

        public async Task<List<int>> GetConversationIdsForUser(string userId)
        {
            return await _unitOfWork.dbContext.ConversationParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.ConversationId)
                .ToListAsync();
        }

        public async Task<MessageDto> SendMessage(string senderId, SendMessageDto dto)
        {
            try
            {
                var participant = await _unitOfWork.dbContext.ConversationParticipants
                    .FirstOrDefaultAsync(p => p.ConversationId == dto.ConversationId && p.UserId == senderId);

                if (participant == null)
                    throw new UnauthorizedAccessException("Non sei un partecipante di questa conversazione");

                if (participant.IsMonitor)
                    throw new UnauthorizedAccessException("I monitor non possono inviare messaggi");

                var now = DateTime.UtcNow;
                var message = new Message
                {
                    ConversationId = dto.ConversationId,
                    SenderId = senderId,
                    Content = dto.Content,
                    Type = (MessageType)dto.Type,
                    CreationDate = now,
                    UpdateDate = now
                };

                await _unitOfWork.dbContext.Messages.AddAsync(message);

                // Mark as read for the sender immediately
                var readStatus = new MessageReadStatus
                {
                    MessageId = message.Id,
                    UserId = senderId,
                    ReadAt = now
                };

                // Update conversation UpdateDate
                var conv = await _unitOfWork.dbContext.Conversations.FindAsync(dto.ConversationId);
                if (conv != null)
                {
                    conv.UpdateDate = now;
                    _unitOfWork.dbContext.Conversations.Update(conv);
                }

                await _unitOfWork.SaveAsync();

                // Now add read status (message.Id is populated after save)
                readStatus.MessageId = message.Id;
                await _unitOfWork.dbContext.MessageReadStatuses.AddAsync(readStatus);
                await _unitOfWork.SaveAsync();

                var sender = await _userManager.FindByIdAsync(senderId);
                return MapMessageToDto(message, sender);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'invio del messaggio");
                throw;
            }
        }

        public async Task MarkAsRead(string userId, MarkReadDto dto)
        {
            try
            {
                var participant = await _unitOfWork.dbContext.ConversationParticipants
                    .FirstOrDefaultAsync(p => p.ConversationId == dto.ConversationId && p.UserId == userId);

                if (participant == null) return;

                // Get all unread messages up to LastMessageId
                var unreadMessages = await _unitOfWork.dbContext.Messages
                    .Where(m => m.ConversationId == dto.ConversationId
                                && m.Id <= dto.LastMessageId
                                && !m.ReadStatuses.Any(r => r.UserId == userId))
                    .ToListAsync();

                var now = DateTime.UtcNow;
                foreach (var msg in unreadMessages)
                {
                    await _unitOfWork.dbContext.MessageReadStatuses.AddAsync(new MessageReadStatus
                    {
                        MessageId = msg.Id,
                        UserId = userId,
                        ReadAt = now
                    });
                }

                participant.LastReadMessageId = dto.LastMessageId;
                participant.LastReadAt = now;
                _unitOfWork.dbContext.ConversationParticipants.Update(participant);

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel marcare i messaggi come letti");
            }
        }

        public async Task<List<MessageDto>> GetMessages(int conversationId, string userId, int page, int pageSize)
        {
            try
            {
                var participant = await _unitOfWork.dbContext.ConversationParticipants
                    .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);

                if (participant == null)
                    throw new UnauthorizedAccessException("Non hai accesso a questa conversazione");

                var messages = await _unitOfWork.dbContext.Messages
                    .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                    .Include(m => m.Sender)
                    .Include(m => m.Attachments)
                    .Include(m => m.ReadStatuses)
                    .OrderByDescending(m => m.CreationDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return messages
                    .OrderBy(m => m.CreationDate)
                    .Select(m => MapMessageToDto(m, m.Sender))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei messaggi per la conversazione {ConvId}", conversationId);
                throw;
            }
        }

        public async Task<List<ContactDto>> GetContacts(string userId)
        {
            try
            {
                var circleIds = await _accessControl.GetCircleUserIdsFor(userId);
                circleIds.Remove(userId);

                var contacts = new List<ContactDto>();
                foreach (var id in circleIds)
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null) continue;
                    var roles = await _userManager.GetRolesAsync(user);
                    contacts.Add(new ContactDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = roles.FirstOrDefault() ?? "User",
                        Color = user.Color,
                        CompanyName = user.CompanyName
                    });
                }

                return contacts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei contatti");
                throw new Exception("Si è verificato un errore nel recupero dei contatti");
            }
        }

        public async Task<List<ConversationDto>> GetMonitoringConversations(string supervisorId)
        {
            try
            {
                var supervisor = await _userManager.FindByIdAsync(supervisorId);
                if (supervisor == null) throw new Exception("Utente non trovato");

                var supervisorRoles = await _userManager.GetRolesAsync(supervisor);
                if (!supervisorRoles.Contains("Admin") && !supervisorRoles.Contains("Agency"))
                    throw new UnauthorizedAccessException("Solo Admin e Agency possono accedere al pannello di monitoraggio");

                var circleIds = await _accessControl.GetCircleUserIdsFor(supervisorId);

                // Get all conversations where at least one participant is in the circle
                var conversations = await _unitOfWork.dbContext.Conversations
                    .Include(c => c.Participants).ThenInclude(p => p.User)
                    .Include(c => c.Messages.OrderByDescending(m => m.CreationDate).Take(1))
                    .Where(c => c.Participants.Any(p => circleIds.Contains(p.UserId) && !p.IsMonitor))
                    .OrderByDescending(c => c.UpdateDate)
                    .ToListAsync();

                var result = new List<ConversationDto>();
                foreach (var conv in conversations)
                {
                    var dto = await MapConversationToDto(conv, supervisorId);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero delle conversazioni di monitoraggio");
                throw;
            }
        }

        public async Task<int> GetTotalUnreadCount(string userId)
        {
            try
            {
                var participations = await _unitOfWork.dbContext.ConversationParticipants
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                int total = 0;
                foreach (var part in participations)
                {
                    var unread = await _unitOfWork.dbContext.Messages
                        .CountAsync(m => m.ConversationId == part.ConversationId
                                        && m.SenderId != userId
                                        && !m.IsDeleted
                                        && !m.ReadStatuses.Any(r => r.UserId == userId));
                    total += unread;
                }

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo dei messaggi non letti");
                return 0;
            }
        }

        // ==================== Private Helpers ====================

        private async Task<Conversation?> FindDirectConversation(string userId1, string userId2)
        {
            var user1Convs = await _unitOfWork.dbContext.ConversationParticipants
                .Where(p => p.UserId == userId1)
                .Select(p => p.ConversationId)
                .ToListAsync();

            return await _unitOfWork.dbContext.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.Type == ConversationType.Direct &&
                    user1Convs.Contains(c.Id) &&
                    c.Participants.Any(p => p.UserId == userId2) &&
                    c.Participants.Count(p => !p.IsMonitor) == 2);
        }

        private async Task AddMonitorsToConversation(int conversationId, string creatorId, DateTime now, List<string> participantIds)
        {
            var creator = await _userManager.FindByIdAsync(creatorId);
            if (creator == null) return;

            var creatorRoles = await _userManager.GetRolesAsync(creator);
            var monitorIds = new HashSet<string>();

            // Collect all superior user IDs
            foreach (var participantId in participantIds.Append(creatorId))
            {
                var participant = await _userManager.FindByIdAsync(participantId);
                if (participant?.AdminId != null)
                    monitorIds.Add(participant.AdminId);

                var pRoles = await _userManager.GetRolesAsync(participant!);
                if (pRoles.Contains("Agency") && !string.IsNullOrEmpty(participant.AdminId))
                    monitorIds.Add(participant.AdminId);
            }

            // Remove participants already in the conversation
            var existingParticipantIds = participantIds.Append(creatorId).ToHashSet();
            monitorIds.ExceptWith(existingParticipantIds);

            foreach (var monitorId in monitorIds)
            {
                await _unitOfWork.dbContext.ConversationParticipants.AddAsync(new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserId = monitorId,
                    IsMonitor = true,
                    CreationDate = now,
                    UpdateDate = now
                });
            }
        }

        private async Task<ConversationDto> MapConversationToDto(Conversation conv, string currentUserId)
        {
            var participants = new List<ParticipantDto>();
            foreach (var p in conv.Participants)
            {
                var roles = p.User != null ? await _userManager.GetRolesAsync(p.User) : new List<string>();
                participants.Add(new ParticipantDto
                {
                    UserId = p.UserId,
                    FirstName = p.User?.FirstName ?? string.Empty,
                    LastName = p.User?.LastName ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "User",
                    Color = p.User?.Color ?? "#ffffff",
                    IsMonitor = p.IsMonitor
                });
            }

            var lastMsg = conv.Messages.OrderByDescending(m => m.CreationDate).FirstOrDefault();
            var lastMsgDto = lastMsg != null ? MapMessageToDto(lastMsg, lastMsg.Sender) : null;

            var unread = await _unitOfWork.dbContext.Messages
                .CountAsync(m => m.ConversationId == conv.Id
                                && m.SenderId != currentUserId
                                && !m.IsDeleted
                                && !m.ReadStatuses.Any(r => r.UserId == currentUserId));

            return new ConversationDto
            {
                Id = conv.Id,
                Title = conv.Title,
                Type = (int)conv.Type,
                CreatedByUserId = conv.CreatedByUserId,
                CreationDate = conv.CreationDate,
                UpdateDate = conv.UpdateDate,
                LastMessage = lastMsgDto,
                UnreadCount = unread,
                Participants = participants
            };
        }

        private static MessageDto MapMessageToDto(Message msg, ApplicationUser? sender)
        {
            return new MessageDto
            {
                Id = msg.Id,
                ConversationId = msg.ConversationId,
                SenderId = msg.SenderId,
                SenderFirstName = sender?.FirstName ?? string.Empty,
                SenderLastName = sender?.LastName ?? string.Empty,
                SenderColor = sender?.Color ?? "#ffffff",
                Content = msg.Content,
                Type = (int)msg.Type,
                IsDeleted = msg.IsDeleted,
                CreationDate = msg.CreationDate,
                Attachments = msg.Attachments?.Select(a => new MessageAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType
                }).ToList() ?? new(),
                ReadByUserIds = msg.ReadStatuses?.Select(r => r.UserId).ToList() ?? new()
            };
        }
    }
}
