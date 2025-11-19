using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CalendarModels;
using BackEnd.Services;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class CalendarController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICalendarServices _calendarServices;
        private readonly ILogger<CalendarController> _logger;
        private readonly AccessControlService _accessControl;
        private readonly ICustomerServices _customerServices;
        private readonly IRealEstatePropertyServices _realEstatePropertyServices;
        private readonly IRequestServices _requestServices;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(
           IConfiguration configuration,
           ICalendarServices calendarServices,
            ILogger<CalendarController> logger,
            AccessControlService accessControl,
            ICustomerServices customerServices,
            IRealEstatePropertyServices realEstatePropertyServices,
            IRequestServices requestServices,
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _calendarServices = calendarServices;
            _logger = logger;
            _accessControl = accessControl;
            _customerServices = customerServices;
            _realEstatePropertyServices = realEstatePropertyServices;
            _requestServices = requestServices;
            _subscriptionLimitService = subscriptionLimitService;
            _userManager = userManager;
        }
        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(CalendarCreateModel request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Valida che le entità associate siano nella cerchia dell'utente
                if (request.CustomerId.HasValue)
                {
                    var customer = await _customerServices.GetById(request.CustomerId.Value);
                    if (customer == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                    
                    bool canAccessCustomer = await _accessControl.CanAccessEntity(currentUserId, customer.UserId);
                    if (!canAccessCustomer)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso al cliente selezionato. Puoi associare solo clienti della tua cerchia." });
                }
                
                if (request.RealEstatePropertyId.HasValue)
                {
                    var property = await _realEstatePropertyServices.GetById(request.RealEstatePropertyId.Value);
                    if (property == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Proprietà immobiliare non trovata" });
                    
                    bool canAccessProperty = await _accessControl.CanAccessEntity(currentUserId, property.UserId);
                    if (!canAccessProperty)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla proprietà selezionata. Puoi associare solo proprietà della tua cerchia." });
                }
                
                if (request.RequestId.HasValue)
                {
                    var req = await _requestServices.GetById(request.RequestId.Value);
                    if (req == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                    
                    bool canAccessRequest = await _accessControl.CanAccessEntity(currentUserId, req.UserId);
                    if (!canAccessRequest)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla richiesta selezionata. Puoi associare solo richieste della tua cerchia." });
                }
                
                CalendarSelectModel Result = await _calendarServices.Create(request);
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
        public async Task<IActionResult> Update(CalendarUpdateModel request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Verifica che l'utente possa modificare questo evento
                var existingEvent = await _calendarServices.GetById(request.Id);
                if (existingEvent == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Evento non trovato" });
                
                bool canModify = await _accessControl.CanModifyEntity(currentUserId, existingEvent.UserId);
                if (!canModify)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questo evento" });
                
                // Valida che le entità associate siano nella cerchia dell'utente
                if (request.CustomerId.HasValue)
                {
                    var customer = await _customerServices.GetById(request.CustomerId.Value);
                    if (customer == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                    
                    bool canAccessCustomer = await _accessControl.CanAccessEntity(currentUserId, customer.UserId);
                    if (!canAccessCustomer)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso al cliente selezionato. Puoi associare solo clienti della tua cerchia." });
                }
                
                if (request.RealEstatePropertyId.HasValue)
                {
                    var property = await _realEstatePropertyServices.GetById(request.RealEstatePropertyId.Value);
                    if (property == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Proprietà immobiliare non trovata" });
                    
                    bool canAccessProperty = await _accessControl.CanAccessEntity(currentUserId, property.UserId);
                    if (!canAccessProperty)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla proprietà selezionata. Puoi associare solo proprietà della tua cerchia." });
                }
                
                if (request.RequestId.HasValue)
                {
                    var req = await _requestServices.GetById(request.RequestId.Value);
                    if (req == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                    
                    bool canAccessRequest = await _accessControl.CanAccessEntity(currentUserId, req.UserId);
                    if (!canAccessRequest)
                        return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla richiesta selezionata. Puoi associare solo richieste della tua cerchia." });
                }
                
                CalendarSelectModel Result = await _calendarServices.Update(request);

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
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                ListViewModel<CalendarSelectModel> res = await _calendarServices.Get(userId, null, null);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }
        
        [HttpGet]
        [Route(nameof(GetToInsert))]
        public async Task<IActionResult> GetToInsert()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                CalendarCreateViewModel res = await _calendarServices.GetToInsert(userId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetSearchItems))]
        public async Task<IActionResult> GetSearchItems(string userId, string? agencyId)
        {
            try
            {
                CalendarSearchModel res = await _calendarServices.GetSearchItems(userId, agencyId);

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
                
                CalendarSelectModel result = await _calendarServices.GetById(id);
                
                if (result == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Evento non trovato" });
                
                // Verifica che l'utente possa accedere a questo evento (deve essere nella cerchia)
                bool canAccess = await _accessControl.CanAccessEntity(currentUserId, result.UserId);
                
                if (!canAccess)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questo evento" });

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
                
                // Recupera l'evento esistente
                var existingEvent = await _calendarServices.GetById(id);
                if (existingEvent == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Evento non trovato" });
                
                // Verifica permessi di eliminazione
                bool canDelete = await _accessControl.CanModifyEntity(currentUserId, existingEvent.UserId);
                if (!canDelete)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questo evento" });
                
                BackEnd.Entities.Calendar result = await _calendarServices.Delete(id);
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
        public async Task<IActionResult> Export([FromBody] CalendarExportModel filters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var permissionResult = await EnsureExportPermissions(userId);
                if (permissionResult != null)
                {
                    return permissionResult;
                }

                var payload = filters ?? new CalendarExportModel();
                var data = await _calendarServices.GetForExportAsync(userId, payload);

                var table = BuildCalendarExportTable(data);
                var format = payload.Format?.ToLowerInvariant() == "csv" ? "csv" : "excel";
                var (contentType, extension) = format == "csv"
                    ? ("text/csv", "csv")
                    : ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

                byte[] fileBytes = format == "csv"
                    ? BackEnd.Services.Export.GenerateCsvContent(table)
                    : BackEnd.Services.Export.GenerateExcelContent(table);

                await _subscriptionLimitService.RecordExportAsync(userId, format, "calendar");

                var fileName = $"calendario_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<IActionResult?> EnsureExportPermissions(string userId)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            var adminId = currentUser?.AdminId;

            bool exportEnabled = await _subscriptionLimitService.IsExportEnabledAsync(userId, adminId);
            if (!exportEnabled)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AuthResponseModel()
                    {
                        Status = "Error",
                        Message = "L'export dei dati non è disponibile nel tuo piano. Aggiorna l'abbonamento per utilizzare questa funzionalità."
                    });
            }

            var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_exports", adminId);
            if (!limitCheck.CanProceed)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, limitCheck);
            }

            return null;
        }

        private static DataTable BuildCalendarExportTable(IEnumerable<CalendarSelectModel> data)
        {
            var table = new DataTable("Eventi");
            table.Columns.Add("Codice");
            table.Columns.Add("Titolo");
            table.Columns.Add("Data Inizio");
            table.Columns.Add("Data Fine");
            table.Columns.Add("Stato");
            table.Columns.Add("Agente");
            table.Columns.Add("Descrizione");

            foreach (var item in data)
            {
                table.Rows.Add(
                    item.Id,
                    item.EventName,
                    item.EventStartDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    item.EventEndDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    ResolveEventStatus(item),
                    $"{item.User?.FirstName} {item.User?.LastName}".Trim(),
                    item.EventDescription);
            }

            return table;
        }

        private static string ResolveEventStatus(CalendarSelectModel item)
        {
            if (item.Cancelled) return "Disdetto";
            if (item.Postponed) return "Rimandato";
            if (item.Confirmed) return "Confermato";
            return "In attesa";
        }
    }
}
