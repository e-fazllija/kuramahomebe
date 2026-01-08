using AutoMapper;
using BackEnd.Entities;
using BackEnd.Exceptions;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.AuthModels;
using BackEnd.Models.MailModels;
using BackEnd.Models.OutputModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.UserModel;
using BackEnd.Models.UserSubscriptionModels;
using BackEnd.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class AgenciesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AgenciesController> _logger;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;

        public AgenciesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
           IConfiguration configuration,
            ILogger<AgenciesController> logger,
            IMapper mapper,
            IMailService mailService,
            ISubscriptionLimitService subscriptionLimitService,
            IUserSubscriptionServices userSubscriptionServices,
            ISubscriptionPlanServices subscriptionPlanServices)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _mailService = mailService;
            _subscriptionLimitService = subscriptionLimitService;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
        }

        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(UserUpdateModel request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await userManager.FindByIdAsync(userId);
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                // Solo Admin può aggiornare le agenzie
                if (!currentUserRoles.Contains("Admin"))
                {
                    return StatusCode(403, "Accesso negato: solo gli admin possono modificare le agenzie");
                }
                
                ApplicationUser user = await userManager.FindByIdAsync(request.Id) ?? throw new NullReferenceException("Agenzia non trovata");
                
                // Admin può aggiornare solo le proprie Agency
                if (user.AdminId != userId)
                {
                    return StatusCode(403, "Accesso negato: puoi modificare solo le tue agenzie");
                }
                
                _mapper.Map(request, user);
                IdentityResult Result = await userManager.UpdateAsync(user);

                if (Result.Succeeded)
                    return Ok();
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = Result.Errors.ToString() ?? "Si è verificato un errore" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<IActionResult> Create([FromBody] UserCreateModel model)
        {
            try
            {
                // Verifica che solo gli Admin possano creare altre agenzie
                var currentUser = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Utente non autorizzato"));
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                if (!currentUserRoles.Contains("Admin"))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per creare altre agenzie. Solo gli admin possono creare nuove agenzie." });
                }

                // Verifica limite subscription PRIMA di creare
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_agencies", currentUser.AdminId);
                
                if (!limitCheck.CanProceed)
                {
                    // Limite raggiunto - ritorna 429 con dettagli
                    return StatusCode(StatusCodes.Status429TooManyRequests, limitCheck);
                }

                // Verifica se l'email esiste già
                var userEmailExists = await userManager.FindByEmailAsync(model.Email);
                if (userEmailExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Email già registrata!" });

                // Genera password random
                string randomPassword = GenerateRandomPassword();
               
                // Crea l'utente
                ApplicationUser user = _mapper.Map<ApplicationUser>(model);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.UserName = user.Email;
                user.AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user.EmailConfirmed = true;

                var result = await userManager.CreateAsync(user, randomPassword);

                if (!result.Succeeded)
                {
                    string errorMessage = "Errore durante la creazione dell'agenzia: ";
                    if (result.Errors.Any())
                        errorMessage += string.Join(", ", result.Errors.Select(e => e.Description));
                    
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = errorMessage });
                }

                // Assegna ruolo "Agency" fisso
                var roleResult = await userManager.AddToRoleAsync(user, "Agency");
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user); // Rollback se l'assegnazione del ruolo fallisce
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Errore durante l'assegnazione del ruolo" });
                }

                // Se è stato specificato un piano di abbonamento, crea l'abbonamento per l'agenzia
                if (model.SubscriptionPlanId.HasValue && model.SubscriptionPlanId.Value > 0)
                {
                    try
                    {
                        // Verifica che il piano esista e sia attivo
                        var plan = await _subscriptionPlanServices.GetByIdAsync(model.SubscriptionPlanId.Value);
                        if (plan == null || !plan.Active)
                        {
                            // Se il piano non esiste o non è attivo, elimina l'agenzia creata
                            await userManager.DeleteAsync(user);
                            return StatusCode(StatusCodes.Status400BadRequest, new AuthResponseModel() { Status = "Error", Message = "Il piano di abbonamento specificato non esiste o non è attivo" });
                        }

                        // Calcola la data di fine abbonamento in base al periodo di fatturazione
                        DateTime? endDate = null;
                        if (plan.BillingPeriod?.ToLower() == "monthly")
                        {
                            endDate = DateTime.UtcNow.AddMonths(1);
                        }
                        else if (plan.BillingPeriod?.ToLower() == "yearly")
                        {
                            endDate = DateTime.UtcNow.AddYears(1);
                        }

                        // Crea l'abbonamento per l'agenzia
                        var subscriptionModel = new UserSubscriptionCreateModel
                        {
                            UserId = user.Id,
                            SubscriptionPlanId = model.SubscriptionPlanId.Value,
                            StartDate = DateTime.UtcNow,
                            EndDate = endDate,
                            Status = "active",
                            AutoRenew = false // L'admin gestirà il rinnovo manualmente
                        };

                        await _userSubscriptionServices.CreateAsync(subscriptionModel);
                        _logger.LogInformation($"Abbonamento creato per l'agenzia {user.Id} con piano {model.SubscriptionPlanId.Value}");
                    }
                    catch (Exception subscriptionEx)
                    {
                        // Se la creazione dell'abbonamento fallisce, elimina l'agenzia creata
                        _logger.LogError(subscriptionEx, $"Errore durante la creazione dell'abbonamento per l'agenzia {user.Id}");
                        await userManager.DeleteAsync(user);
                        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = $"Errore durante la creazione dell'abbonamento: {subscriptionEx.Message}" });
                    }
                }

                // ===== MODALITÀ TEST: Comunicazione credenziali =====
                Console.WriteLine("========================================");
                Console.WriteLine("CREAZIONE AGENZIA (TEST):");
                Console.WriteLine($"EMAIL: {user.Email}");
                Console.WriteLine($"PASSWORD TEMPORANEA: {randomPassword}");
                Console.WriteLine("========================================");

                // ===== MODALITÀ PRODUZIONE: Invio email con credenziali =====
                // Decommentare le righe seguenti per l'invio effettivo delle email in produzione
                // MailRequest mailRequest = new MailRequest()
                // {
                //     ToEmail = user.Email,
                //     Subject = "Benvenuto in KuramaHome - Credenziali Agenzia",
                //     Body = $@"
                //         <h2>Benvenuto in KuramaHome!</h2>
                //         <p>La tua agenzia è stata creata con successo.</p>
                //         <p><strong>Credenziali temporanee:</strong></p>
                //         <p>Email: {user.Email}</p>
                //         <p>Password: {randomPassword}</p>
                //         <p><strong>IMPORTANTE:</strong> Cambia la password al primo accesso per motivi di sicurezza.</p>
                //         <p>Per accedere alla piattaforma, visita il portale KuramaHome e inserisci le tue credenziali.</p>
                //     "
                // };
                // await _mailService.SendEmailAsync(mailRequest);

                return Ok(new AuthResponseModel { Status = "Success", Message = "Agenzia creata con successo!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore durante la creazione dell'agenzia: " + ex.Message });
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            // Assicura che la password contenga almeno un carattere di ogni tipo richiesto
            return password + "A1!"; // Aggiunge maiuscola, numero e carattere speciale
        }

        [HttpGet]
        [Route(nameof(Get))]
        public async Task<IActionResult> Get(int currentPage, string? filterRequest)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await userManager.FindByIdAsync(userId);
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                // Controllo: gli Agent non possono vedere le agenzie
                if (currentUserRoles.Contains("Agent"))
                {
                    return StatusCode(403, "Accesso negato: gli agenti non possono visualizzare le agenzie");
                }
                
                //currentPage = currentPage > 0 ? currentPage : 1;
                var usersList = await userManager.GetUsersInRoleAsync("Agency");
                usersList = usersList.Where(x => x.AdminId == userId).ToList();


                if (!string.IsNullOrEmpty(filterRequest))
                    usersList = usersList.Where(x => x.Email.Contains(filterRequest)).ToList();

                List<ApplicationUser> users = usersList.ToList();
                ListViewModel<UserSelectModel> result = new ListViewModel<UserSelectModel>();

                result.Total = users.Count();
                result.Data = _mapper.Map<List<UserSelectModel>>(users);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await userManager.FindByIdAsync(userId);
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                // Agency e Agent non possono vedere le agenzie
                if (currentUserRoles.Contains("Agency") || currentUserRoles.Contains("Agent"))
                {
                    return StatusCode(403, "Accesso negato");
                }
                
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new AuthResponseModel() { Status = "Error", Message = "Agenzia non trovata" });
                }
                
                // Admin può vedere solo le proprie Agency
                if (currentUserRoles.Contains("Admin") && user.AdminId != userId)
                {
                    return StatusCode(403, "Accesso negato: puoi visualizzare solo le tue agenzie");
                }
                
                UserSelectModel result = _mapper.Map<UserSelectModel>(user);
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
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await userManager.FindByIdAsync(userId);
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                // Solo Admin può eliminare le agenzie
                if (!currentUserRoles.Contains("Admin"))
                {
                    return StatusCode(403, "Accesso negato: solo gli admin possono eliminare le agenzie");
                }
                
                ApplicationUser? user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Agenzia non trovata" });
                }
                
                // Admin può eliminare solo le proprie Agency
                if (user.AdminId != userId)
                {
                    return StatusCode(403, "Accesso negato: puoi eliminare solo le tue agenzie");
                }
                
                // Rimuovi eventuali utenti collegati (es. agenti) per evitare violazioni FK
                // Nota: anche questi verranno eliminati a cascata se configurato, ma UserManager.DeleteAsync
                // gestisce anche le tabelle di Identity (AspNetUserRoles, etc.)
                var dependentUsers = await userManager.Users.Where(u => u.AdminId == id).ToListAsync();

                foreach (var dependent in dependentUsers)
                {
                    var deleteDependentResult = await userManager.DeleteAsync(dependent);
                    if (!deleteDependentResult.Succeeded)
                    {
                        var errorMessage = string.Join(", ", deleteDependentResult.Errors.Select(e => e.Description));
                        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = $"Impossibile eliminare gli utenti collegati: {errorMessage}" });
                    }
                }

                // Elimina l'agenzia stessa
                // Tutte le entità correlate verranno eliminate automaticamente dal database grazie a DeleteBehavior.Cascade
                var deleteResult = await userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = $"Impossibile eliminare l'agenzia: {errorMessage}" });
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'agenzia {AgencyId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = $"Errore durante l'eliminazione: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route(nameof(Export))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export([FromBody] AgencyExportModel filters)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var permissionResult = _subscriptionLimitService.EnsureExportPermissions(currentUserId);

                var payload = filters ?? new AgencyExportModel();
                var agencies = await GetAgenciesForExportAsync(currentUserId, payload);

                var table = BuildAgenciesExportTable(agencies);
                var format = payload.Format?.ToLowerInvariant() == "csv" ? "csv" : "excel";
                var (contentType, extension) = format == "csv"
                    ? ("text/csv", "csv")
                    : ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

                byte[] fileBytes = format == "csv"
                    ? BackEnd.Services.Export.GenerateCsvContent(table)
                    : BackEnd.Services.Export.GenerateExcelContent(table);

                await _subscriptionLimitService.RecordExportAsync(currentUserId, format, "agencies");

                var fileName = $"agenzie_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<List<UserSelectModel>> GetAgenciesForExportAsync(string adminId, AgencyExportModel filters)
        {
            var agenciesList = await userManager.GetUsersInRoleAsync("Agency");
            agenciesList = agenciesList.Where(x => x.AdminId == adminId).ToList();

            if (filters?.OnlyActive == true)
            {
                agenciesList = agenciesList.Where(x => x.EmailConfirmed).ToList();
            }

            if (!string.IsNullOrEmpty(filters?.Search))
            {
                var lowered = filters.Search.ToLower();
                agenciesList = agenciesList.Where(x =>
                    (x.FirstName ?? string.Empty).ToLower().Contains(lowered) ||
                    (x.LastName ?? string.Empty).ToLower().Contains(lowered) ||
                    x.Email.ToLower().Contains(lowered)).ToList();
            }

            return _mapper.Map<List<UserSelectModel>>(agenciesList);
        }

        private static DataTable BuildAgenciesExportTable(IEnumerable<UserSelectModel> agencies)
        {
            var table = new DataTable("Agenzie");
            table.Columns.Add("Codice");
            table.Columns.Add("Nome");
            table.Columns.Add("Cognome");
            table.Columns.Add("Email");
            table.Columns.Add("Telefono");
            table.Columns.Add("Attiva");

            foreach (var agency in agencies)
            {
                table.Rows.Add(
                    agency.Id,
                    agency.FirstName,
                    agency.LastName,
                    agency.Email,
                    agency.PhoneNumber,
                    agency.EmailConfirmed ? "Sì" : "No");
            }

            return table;
        }
    }
}
