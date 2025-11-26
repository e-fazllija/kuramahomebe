using Microsoft.AspNetCore.Mvc;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RealEstatePropertyPhotoModels;
using BackEnd.Models.CalendarModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BackEnd.Entities;
using System.Security.Claims;
using BackEnd.Models.SubscriptionLimitModels;
using BackEnd.Services;
using System.Data;
using System.Globalization;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class RealEstatePropertyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IRealEstatePropertyServices _realEstatePropertyServices;
        private readonly IRealEstatePropertyPhotoServices _realEstatePropertyPhotoServices;
        private readonly ILogger<RealEstatePropertyController> _logger;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;

        public RealEstatePropertyController(
           IConfiguration configuration,
           IRealEstatePropertyServices realEstatePropertyServices,
           IRealEstatePropertyPhotoServices realEstatePropertyPhotoServices,
            ILogger<RealEstatePropertyController> logger,
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControl)
        {
            _configuration = configuration;
            _realEstatePropertyServices = realEstatePropertyServices;
            _realEstatePropertyPhotoServices = realEstatePropertyPhotoServices;
            _logger = logger;
            _subscriptionLimitService = subscriptionLimitService;
            _userManager = userManager;
            _accessControl = accessControl;
        }

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create([FromForm] RealEstatePropertyCreateModel request)
        {
            try
            {
                // Verifica limite subscription PRIMA di creare
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Utente non trovato" });

                var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_properties", currentUser.AdminId);

                if (!limitCheck.CanProceed)
                {
                    // Limite raggiunto - ritorna 429 con dettagli
                    return StatusCode(StatusCodes.Status429TooManyRequests, limitCheck);
                }

                // Log per debug: verifica che AssignmentEnd sia valida (la conversione a UTC è gestita nel servizio)
                _logger.LogInformation($"AssignmentEnd ricevuta: {request.AssignmentEnd}, Kind: {request.AssignmentEnd.Kind}");

                RealEstatePropertySelectModel Result = await _realEstatePropertyServices.Create(request);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella creazione dell'immobile: {ex.Message}");

                // Log dell'inner exception se presente (per DbUpdateException)
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.StackTrace != null)
                    {
                        _logger.LogError($"Stack Trace: {ex.InnerException.StackTrace}");
                    }
                }

                // Messaggio dettagliato per il client
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Dettagli: {ex.InnerException.Message}";
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = errorMessage });
            }
        }

        [HttpPost]
        [Route(nameof(UploadFiles))]
        public async Task<IActionResult> UploadFiles(UploadFilesModel request)
        {
            try
            {
                await _realEstatePropertyServices.InsertFiles(request);
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
        public async Task<IActionResult> Update(RealEstatePropertyUpdateModel request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Recupera la proprietà esistente
                var property = await _realEstatePropertyServices.GetById(request.Id);
                if (property == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Proprietà non trovata" });
                
                // Verifica permessi di modifica usando AccessControlService
                bool canModify = await _accessControl.CanModifyEntity(currentUserId, property.UserId);
                
                if (!canModify)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questa proprietà" });
                
                RealEstatePropertySelectModel Result = await _realEstatePropertyServices.Update(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(UpdatePhotosOrder))]
        public async Task<IActionResult> UpdatePhotosOrder(List<RealEstatePropertyPhotoUpdateModel> request)
        {
            try
            {
                List<RealEstatePropertyPhotoSelectModel> result = await _realEstatePropertyPhotoServices.UpdateOrder(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetMain))]
        public async Task<IActionResult> GetMain(int currentPage, string? filterRequest, string? status, string? typologie, string? location, int? code, int? from, int? to, string? agencyId, string? city)
        {
            try
            {
                ListViewModel<RealEstatePropertySelectModel> res = await _realEstatePropertyServices.Get(currentPage, filterRequest, status, typologie, location,
                 code,
                 from,
                 to,
                 agencyId,
                 city, null);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(int currentPage, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                ListViewModel<RealEstatePropertySelectModel> res = await _realEstatePropertyServices.Get(
                    currentPage, filterRequest, contract, priceFrom, priceTo, category, typologie, city, userId);

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
        public async Task<IActionResult> GetList(int currentPage, string? filterRequest, string? contract, int? priceFrom, int? priceTo, string? category, string? typologie, string? city, bool? sold)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                ListViewModel<RealEstatePropertyListModel> res = await _realEstatePropertyServices.GetList(
                    currentPage, filterRequest, contract, priceFrom, priceTo, category, typologie, city, sold, userId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetPropertyCount))]
        public IActionResult GetPropertyCount()
        {
            try
            {
                int res = _realEstatePropertyServices.GetPropertyCount();

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

                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Utente non autenticato" });
                }

                RealEstatePropertyCreateViewModel res = await _realEstatePropertyServices.GetToInsert(userId);

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
                
                RealEstatePropertySelectModel result = await _realEstatePropertyServices.GetById(id);
                
                if (result == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Proprietà non trovata" });
                
                // Verifica che l'utente possa accedere a questa proprietà (deve essere nella cerchia)
                bool canAccess = await _accessControl.CanAccessEntity(currentUserId, result.UserId);
                
                if (!canAccess)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questa proprietà" });

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
                
                // Recupera la proprietà esistente
                var property = await _realEstatePropertyServices.GetById(id);
                if (property == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Proprietà non trovata" });
                
                // Verifica permessi di eliminazione usando AccessControlService
                bool canDelete = await _accessControl.CanModifyEntity(currentUserId, property.UserId);
                
                if (!canDelete)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questa proprietà" });
                
                await _realEstatePropertyServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpDelete]
        [Route(nameof(DeletePhoto))]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                await _realEstatePropertyPhotoServices.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(SetHighlighted))]
        public async Task<IActionResult> SetHighlighted(int realEstatePropertyId)
        {
            try
            {
                await _realEstatePropertyServices.SetHighlighted(realEstatePropertyId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(SetRealEstatePropertyPhotoHighlighted))]
        public async Task<IActionResult> SetRealEstatePropertyPhotoHighlighted([FromForm] int realEstatePropertyPhotoId)
        {
            try
            {
                await _realEstatePropertyPhotoServices.SetHighlighted(realEstatePropertyPhotoId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(SetInHome))]
        public async Task<IActionResult> SetInHome(int realEstatePropertyId)
        {
            try
            {
                await _realEstatePropertyServices.SetInHome(realEstatePropertyId);

                return Ok();
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
                CalendarSearchModel res = await _realEstatePropertyServices.GetSearchItems(userId, agencyId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Export))]
        public async Task<IActionResult> Export([FromBody] RealEstatePropertyExportModel filters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var permissionResult = _subscriptionLimitService.EnsureExportPermissions(userId);

                var payload = filters ?? new RealEstatePropertyExportModel();
                var data = await _realEstatePropertyServices.GetListForExportAsync(payload, userId);

                var table = BuildPropertyExportTable(data);
                var format = payload.Format?.ToLowerInvariant() == "csv" ? "csv" : "excel";
                var (contentType, extension) = format == "csv"
                    ? ("text/csv", "csv")
                    : ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

                byte[] fileBytes = format == "csv"
                    ? BackEnd.Services.Export.GenerateCsvContent(table)
                    : BackEnd.Services.Export.GenerateExcelContent(table);

                await _subscriptionLimitService.RecordExportAsync(userId, format, "properties");

                var fileName = $"immobili_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private static DataTable BuildPropertyExportTable(IEnumerable<RealEstatePropertyListModel> data)
        {
            var culture = CultureInfo.GetCultureInfo("it-IT");
            var table = new DataTable("Immobili");
            table.Columns.Add("Codice");
            table.Columns.Add("Data Inserimento");
            table.Columns.Add("Fine Incarico");
            table.Columns.Add("Indirizzo");
            table.Columns.Add("Città");
            table.Columns.Add("Provincia");
            table.Columns.Add("Prezzo (€)");
            table.Columns.Add("Categoria");
            table.Columns.Add("Tipologia");
            table.Columns.Add("Stato Immobile");
            table.Columns.Add("Stato Incarico");
            table.Columns.Add("Asta");
            table.Columns.Add("Venduto");

            foreach (var item in data)
            {
                table.Rows.Add(
                    item.Id,
                    item.CreationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    item.AssignmentEnd.ToLocalTime().ToString("dd/MM/yyyy"),
                    item.AddressLine,
                    item.City,
                    item.State,
                    item.Price.ToString("N0", culture),
                    item.Category,
                    item.Typology,
                    item.StateOfTheProperty,
                    item.Status,
                    item.Auction ? "Sì" : "No",
                    item.Sold ? "Sì" : "No");
            }

            return table;
        }
    }
}
