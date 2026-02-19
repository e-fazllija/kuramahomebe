using BackEnd.Entities;
using BackEnd.Hubs;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ChatModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]/")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationServices _conversationServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationServices conversationServices,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext,
            ILogger<ConversationsController> logger)
        {
            _conversationServices = conversationServices;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var conversations = await _conversationServices.GetConversationsForUser(userId!);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var conversation = await _conversationServices.GetConversationById(id, userId!);
                return Ok(conversation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create([FromBody] CreateConversationDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var conversation = await _conversationServices.CreateConversation(userId!, dto);

                // Notify all participants about the new conversation via SignalR
                foreach (var participant in conversation.Participants)
                {
                    await _hubContext.Clients.Group($"user_{participant.UserId}")
                        .SendAsync("NewConversation", conversation);
                    await _hubContext.Clients.Group($"user_{participant.UserId}")
                        .SendAsync("JoinConversationGroup", conversation.Id);
                }

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetMessages))]
        public async Task<IActionResult> GetMessages(int conversationId, int page = 1, int pageSize = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _conversationServices.GetMessages(conversationId, userId!, page, pageSize);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetContacts))]
        public async Task<IActionResult> GetContacts()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var contacts = await _conversationServices.GetContacts(userId!);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetMonitoring))]
        public async Task<IActionResult> GetMonitoring()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId!);
                var roles = await _userManager.GetRolesAsync(currentUser!);

                if (!roles.Contains("Admin") && !roles.Contains("Agency"))
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel { Status = "Error", Message = "Accesso negato" });

                var conversations = await _conversationServices.GetMonitoringConversations(userId!);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetUnreadCount))]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var count = await _conversationServices.GetTotalUnreadCount(userId!);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(MarkRead))]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _conversationServices.MarkAsRead(userId!, dto);

                await _hubContext.Clients.Group($"conv_{dto.ConversationId}").SendAsync("MessagesRead", new
                {
                    UserId = userId,
                    dto.ConversationId,
                    dto.LastMessageId
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }
    }
}
