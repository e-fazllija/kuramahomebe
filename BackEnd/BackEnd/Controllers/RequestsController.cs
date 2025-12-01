using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.RequestModels;
using BackEnd.Services;
using System.Data;
using System.Globalization;
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
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questa richiesta" });
                
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
        public async Task<IActionResult> Get([FromQuery] int currentPage, [FromQuery] string? filterRequest = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                ListViewModel<RequestSelectModel> res = await _requestServices.Get(currentPage, filterRequest, null, null, userId);

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
        public async Task<IActionResult> GetList([FromQuery] int currentPage, [FromQuery] string? filterRequest = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                ListViewModel<RequestListModel> res = await _requestServices.GetList(currentPage, filterRequest, null, null, userId);

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
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questa richiesta" });

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
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questa richiesta" });
                
                Request result = await _requestServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        [HttpPost]
        [Route(nameof(Export))]
        public async Task<IActionResult> Export([FromBody] RequestExportModel filters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var permissionResult = _subscriptionLimitService.EnsureExportPermissions(userId);

                var payload = filters ?? new RequestExportModel();
                var data = await _requestServices.GetForExportAsync(payload, userId);

                var table = BuildRequestExportTable(data);
                var format = payload.Format?.ToLowerInvariant() == "csv" ? "csv" : "excel";
                var (contentType, extension) = format == "csv"
                    ? ("text/csv", "csv")
                    : ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

                byte[] fileBytes = format == "csv"
                    ? BackEnd.Services.Export.GenerateCsvContent(table)
                    : BackEnd.Services.Export.GenerateExcelContent(table);

                await _subscriptionLimitService.RecordExportAsync(userId, format, "requests");

                var fileName = $"richieste_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private static DataTable BuildRequestExportTable(IEnumerable<RequestListModel> data)
        {
            var culture = CultureInfo.GetCultureInfo("it-IT");
            var table = new DataTable("Richieste");
            table.Columns.Add("Codice");
            table.Columns.Add("Cliente");
            table.Columns.Add("Email");
            table.Columns.Add("Telefono");
            table.Columns.Add("Contratto");
            table.Columns.Add("Tipologia Immobile");
            table.Columns.Add("Città");
            table.Columns.Add("Budget Min (€)");
            table.Columns.Add("Budget Max (€)");
            table.Columns.Add("Data Creazione");
            table.Columns.Add("Stato");

            foreach (var item in data)
            {
                table.Rows.Add(
                    item.Id,
                    $"{item.CustomerName} {item.CustomerLastName}".Trim(),
                    item.CustomerEmail,
                    item.CustomerPhone,
                    item.Contract,
                    item.PropertyType,
                    item.City,
                    item.PriceFrom.ToString("N0", culture),
                    item.PriceTo.ToString("N0", culture),
                    item.CreationDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    ResolveRequestStatus(item));
            }

            return table;
        }

        private static string ResolveRequestStatus(RequestListModel item)
        {
            if (item.Archived)
            {
                return "Archiviata";
            }

            if (item.Closed)
            {
                return "Chiusa";
            }

            return "Aperta";
        }
    }
}
