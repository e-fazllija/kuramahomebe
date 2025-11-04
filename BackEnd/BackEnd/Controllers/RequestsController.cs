using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.RequestModels;
using BackEnd.Services;
using System.Data;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BackEnd.Interfaces.IBusinessServices;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class RequestsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IRequestServices _requestServices;
        private readonly ILogger<RequestsController> _logger;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;

        public RequestsController(
           IConfiguration configuration,
           IRequestServices requestServices,
            ILogger<RequestsController> logger,
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControl)
        {
            _configuration = configuration;
            _requestServices = requestServices;
            _logger = logger;
            _subscriptionLimitService = subscriptionLimitService;
            _userManager = userManager;
            _accessControl = accessControl;
        }
        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(RequestCreateModel request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);
                var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_requests", currentUser?.AdminId);
                if (!limitCheck.CanProceed)
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests, limitCheck);
                }
                // ownership
                request.UserId = userId;
                RequestSelectModel Result = await _requestServices.Create(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(RequestUpdateModel request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Recupera la richiesta esistente
                var req = await _requestServices.GetById(request.Id);
                if (req == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                
                // Verifica permessi di modifica usando AccessControlService
                bool canModify = await _accessControl.CanModifyEntity(currentUserId, req.UserId);
                
                if (!canModify)
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questa richiesta" });
                
                RequestSelectModel Result = await _requestServices.Update(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get([FromQuery] int currentPage, [FromQuery] string? agencyId = null, [FromQuery] string? filterRequest = null, [FromQuery] string? userId = null)
        {
            try
            {
                //currentPage = currentPage > 0 ? currentPage : 1;
                ListViewModel<RequestSelectModel> res = await _requestServices.Get(currentPage, agencyId, filterRequest, null, null, userId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetList))]
        public async Task<IActionResult> GetList([FromQuery] int currentPage, [FromQuery] string? agencyId = null, [FromQuery] string? filterRequest = null, [FromQuery] string? userId = null)
        {
            try
            {
                ListViewModel<RequestListModel> res = await _requestServices.GetList(currentPage, agencyId, filterRequest, null, null, userId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetCustomerRequests))]
        public async Task<IActionResult> GetCustomerRequests(int customerId)
        {
            try
            {
                ListViewModel<RequestSelectModel> res = await _requestServices.GetCustomerRequests(customerId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                RequestSelectModel result = await _requestServices.GetById(id);
                
                if (result == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                
                // Verifica che l'utente possa accedere a questa richiesta (deve essere nella cerchia)
                bool canAccess = await _accessControl.CanAccessEntity(currentUserId, result.UserId);
                
                if (!canAccess)
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questa richiesta" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Recupera la richiesta esistente
                var request = await _requestServices.GetById(id);
                if (request == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                
                // Verifica permessi di eliminazione usando AccessControlService
                bool canDelete = await _accessControl.CanModifyEntity(currentUserId, request.UserId);
                
                if (!canDelete)
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questa richiesta" });
                
                Request result = await _requestServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpGet]
        [Route(nameof(ExportExcel))]
        public async Task<IActionResult> ExportExcel(char? fromName, char? toName)
        {
            try
            {
                var result = await _requestServices.Get(0, null, null, fromName, toName, null);
                DataTable table = Export.ToDataTable<RequestSelectModel>(result.Data);
                byte[] fileBytes = Export.GenerateExcelContent(table);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Output.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpGet]
        [Route(nameof(ExportCsv))]
        public async Task<IActionResult> ExportCsv(char? fromName, char? toName)
        {
            try
            {
                var result = await _requestServices.Get(0, null, null, fromName, toName, null);
                DataTable table = Export.ToDataTable<RequestSelectModel>(result.Data);
                byte[] fileBytes = Export.GenerateCsvContent(table);

                return File(fileBytes, "text/csv", "Output.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
    }
}
