using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Services;
using System.Collections.Generic;
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
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICustomerServices _customerServices;
        private readonly ILogger<CustomersController> _logger;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;

        public CustomersController(
           IConfiguration configuration,
           ICustomerServices customerServices,
            ILogger<CustomersController> logger,
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControl)
        {
            _configuration = configuration;
            _customerServices = customerServices;
            _logger = logger;
            _subscriptionLimitService = subscriptionLimitService;
            _userManager = userManager;
            _accessControl = accessControl;
        }
        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create(CustomerCreateModel request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);
                var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_customers", currentUser?.AdminId);
                if (!limitCheck.CanProceed)
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests, limitCheck);
                }
                // ownership
                request.UserId = userId;
                CustomerSelectModel Result = await _customerServices.Create(request);
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
        public async Task<IActionResult> Update(CustomerUpdateModel request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                
                // Recupera il cliente esistente
                var customer = await _customerServices.GetById(request.Id);
                if (customer == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                
                // Verifica permessi di modifica usando AccessControlService
                bool canModify = await _accessControl.CanModifyEntity(currentUserId, customer.UserId);
                
                if (!canModify)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per modificare questo cliente" });
                
                CustomerSelectModel Result = await _customerServices.Update(request);

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
        public async Task<IActionResult> Get([FromQuery] string? filterRequest = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // AccessControlService gestisce automaticamente la cerchia in base al userId
                ListViewModel<CustomerSelectModel> res = await _customerServices.Get(
                    userId, 
                    filterRequest,
                    null, null);

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
                
                CustomerSelectModel result = await _customerServices.GetById(id);
                
                if (result == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                
                // Verifica che l'utente possa accedere a questo cliente (deve essere nella cerchia)
                bool canAccess = await _accessControl.CanAccessEntity(currentUserId, result.UserId);
                
                if (!canAccess)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai accesso a questo cliente" });

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
                
                // Recupera il cliente esistente
                var customer = await _customerServices.GetById(id);
                if (customer == null)
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Cliente non trovato" });
                
                // Verifica permessi di eliminazione usando AccessControlService
                bool canDelete = await _accessControl.CanModifyEntity(currentUserId, customer.UserId);
                
                if (!canDelete)
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per eliminare questo cliente" });
                
                Customer result = await _customerServices.Delete(id);
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
        public async Task<IActionResult> Export([FromBody] CustomerExportModel filters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var permissionResult = _subscriptionLimitService.EnsureExportPermissions(userId);

                var payload = filters ?? new CustomerExportModel();
                var data = await _customerServices.GetForExportAsync(payload, userId);

                var table = BuildCustomerExportTable(data);
                var format = payload.Format?.ToLowerInvariant() == "csv" ? "csv" : "excel";
                var (contentType, extension) = format == "csv"
                    ? ("text/csv", "csv")
                    : ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

                byte[] fileBytes = format == "csv"
                    ? BackEnd.Services.Export.GenerateCsvContent(table)
                    : BackEnd.Services.Export.GenerateExcelContent(table);

                await _subscriptionLimitService.RecordExportAsync(userId, format, "customers");

                var fileName = $"clienti_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private static DataTable BuildCustomerExportTable(IEnumerable<CustomerSelectModel> data)
        {
            var culture = CultureInfo.GetCultureInfo("it-IT");
            var table = new DataTable("Clienti");
            table.Columns.Add("Codice");
            table.Columns.Add("Nome");
            table.Columns.Add("Cognome");
            table.Columns.Add("Email");
            table.Columns.Add("Telefono");
            table.Columns.Add("Tipologie");
            table.Columns.Add("Città");
            table.Columns.Add("Provincia");
            table.Columns.Add("Cliente Gold");
            table.Columns.Add("Data Creazione");
            table.Columns.Add("Acquisizione");
            table.Columns.Add("Incarico in corso");

            foreach (var item in data)
            {
                table.Rows.Add(
                    item.Id,
                    item.FirstName,
                    item.LastName,
                    item.Email,
                    item.Phone.ToString(culture),
                    BuildCustomerTypes(item),
                    item.City,
                    item.State,
                    item.GoldCustomer ? "Sì" : "No",
                    item.CreationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    item.AcquisitionDone ? "Confermata" : "In corso",
                    item.OngoingAssignment ? "Sì" : "No");
            }

            return table;
        }

        private static string BuildCustomerTypes(CustomerSelectModel customer)
        {
            var tags = new List<string>();
            if (customer.Buyer) tags.Add("Compratore");
            if (customer.Seller) tags.Add("Venditore");
            if (customer.Builder) tags.Add("Costruttore");
            if (customer.GoldCustomer) tags.Add("Cliente gold");

            return string.Join(", ", tags);
        }
    }
}
