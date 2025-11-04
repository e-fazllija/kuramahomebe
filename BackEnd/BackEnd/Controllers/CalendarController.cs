using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CalendarModels;
using BackEnd.Services;
using System.Data;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using Microsoft.AspNetCore.Authorization;
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

        public CalendarController(
           IConfiguration configuration,
           ICalendarServices calendarServices,
            ILogger<CalendarController> logger,
            AccessControlService accessControl,
            ICustomerServices customerServices,
            IRealEstatePropertyServices realEstatePropertyServices,
            IRequestServices requestServices)
        {
            _configuration = configuration;
            _calendarServices = calendarServices;
            _logger = logger;
            _accessControl = accessControl;
            _customerServices = customerServices;
            _realEstatePropertyServices = realEstatePropertyServices;
            _requestServices = requestServices;
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
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso al cliente selezionato. Puoi associare solo clienti della tua cerchia." });
                }
                
                if (request.RealEstatePropertyId.HasValue)
                {
                    var property = await _realEstatePropertyServices.GetById(request.RealEstatePropertyId.Value);
                    if (property == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Proprietà immobiliare non trovata" });
                    
                    bool canAccessProperty = await _accessControl.CanAccessEntity(currentUserId, property.UserId);
                    if (!canAccessProperty)
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla proprietà selezionata. Puoi associare solo proprietà della tua cerchia." });
                }
                
                if (request.RequestId.HasValue)
                {
                    var req = await _requestServices.GetById(request.RequestId.Value);
                    if (req == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                    
                    bool canAccessRequest = await _accessControl.CanAccessEntity(currentUserId, req.UserId);
                    if (!canAccessRequest)
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla richiesta selezionata. Puoi associare solo richieste della tua cerchia." });
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
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questo evento" });
                
                // Valida che le entità associate siano nella cerchia dell'utente
                if (request.CustomerId.HasValue)
                {
                    var customer = await _customerServices.GetById(request.CustomerId.Value);
                    if (customer == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                    
                    bool canAccessCustomer = await _accessControl.CanAccessEntity(currentUserId, customer.UserId);
                    if (!canAccessCustomer)
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso al cliente selezionato. Puoi associare solo clienti della tua cerchia." });
                }
                
                if (request.RealEstatePropertyId.HasValue)
                {
                    var property = await _realEstatePropertyServices.GetById(request.RealEstatePropertyId.Value);
                    if (property == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Proprietà immobiliare non trovata" });
                    
                    bool canAccessProperty = await _accessControl.CanAccessEntity(currentUserId, property.UserId);
                    if (!canAccessProperty)
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla proprietà selezionata. Puoi associare solo proprietà della tua cerchia." });
                }
                
                if (request.RequestId.HasValue)
                {
                    var req = await _requestServices.GetById(request.RequestId.Value);
                    if (req == null)
                        return BadRequest(new AuthResponseModel() { Status = "Error", Message = "Richiesta non trovata" });
                    
                    bool canAccessRequest = await _accessControl.CanAccessEntity(currentUserId, req.UserId);
                    if (!canAccessRequest)
                        return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso alla richiesta selezionata. Puoi associare solo richieste della tua cerchia." });
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
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questo evento" });

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
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questo evento" });
                
                Calendar result = await _calendarServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }        
    }
}
