using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.CustomerModels;
using BackEnd.Services;
using System.Data;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.OutputModels;
using AutoMapper;
using BackEnd.Models.UserModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BackEnd.Models.AuthModels;
using BackEnd.Models.MailModels;
using System.Security.Claims;
using BackEnd.Interfaces;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class AgentsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AgenciesController> _logger;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly ISubscriptionLimitService _subscriptionLimitService;

        public AgentsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
           IConfiguration configuration,
            ILogger<AgenciesController> logger,
            IMapper mapper,
            IMailService mailService,
            ISubscriptionLimitService subscriptionLimitService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _mailService = mailService;
            _subscriptionLimitService = subscriptionLimitService;
        }

        [HttpPost]
        [Route(nameof(Update))]
        public async Task<IActionResult> Update(UserUpdateModel request)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(request.Id) ?? throw new NullReferenceException("Agente non trovato");
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
                // Verifica che solo Admin e Agency possano creare agenti (non gli Agent)
                var currentUser = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Utente non autorizzato"));
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);
                
                if (!currentUserRoles.Contains("Admin") && !currentUserRoles.Contains("Agency"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new AuthResponseModel() { Status = "Error", Message = "Non hai i permessi per creare agenti. Solo gli admin e le agenzie possono creare nuovi agenti." });
                }

                // Verifica limite subscription PRIMA di creare
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var limitCheck = await _subscriptionLimitService.CheckFeatureLimitAsync(userId, "max_agents", currentUser.AgencyId);
                
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
                user.UserName = user.Email.Split("@")[0];
                user.AgencyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await userManager.CreateAsync(user, randomPassword);

                if (!result.Succeeded)
                {
                    string errorMessage = "Errore durante la creazione dell'agente: ";
                    if (result.Errors.Any())
                        errorMessage += string.Join(", ", result.Errors.Select(e => e.Description));
                    
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = errorMessage });
                }

                // Assegna ruolo "Agent" fisso
                var roleResult = await userManager.AddToRoleAsync(user, "Agent");
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user); // Rollback se l'assegnazione del ruolo fallisce
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Errore durante l'assegnazione del ruolo" });
                }

                // Genera token per conferma email
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // ===== MODALITÀ TEST: Link di conferma email =====
                var confirmationLink = $"http://localhost:5173/email-confirmation/{user.Email}/{token}";
                Console.WriteLine("========================================");
                Console.WriteLine("LINK DI CONFERMA AGENTE (TEST):");
                Console.WriteLine(confirmationLink);
                Console.WriteLine($"PASSWORD TEMPORANEA: {randomPassword}");
                Console.WriteLine("========================================");

                // ===== MODALITÀ PRODUZIONE: Invio email con credenziali =====
                // Decommentare le righe seguenti per l'invio effettivo delle email in produzione
                // var confirmationLink = $"https://www.amministrazionethinkhome.it/#/email-confirmation/{user.Email}/{token}";
                // MailRequest mailRequest = new MailRequest()
                // {
                //     ToEmail = user.Email,
                //     Subject = "Benvenuto in KuramaHome - Credenziali Agente",
                //     Body = $@"
                //         <h2>Benvenuto in KuramaHome!</h2>
                //         <p>Il tuo account agente è stato creato con successo.</p>
                //         <p><strong>Credenziali temporanee:</strong></p>
                //         <p>Email: {user.Email}</p>
                //         <p>Password: {randomPassword}</p>
                //         <p><strong>IMPORTANTE:</strong> Cambia la password al primo accesso per motivi di sicurezza.</p>
                //         <p>Per attivare il tuo account, <a href='{confirmationLink}'>clicca qui</a></p>
                //         <p>Se il link non funziona, copia e incolla questo URL nel tuo browser:</p>
                //         <p>{confirmationLink}</p>
                //         <p>Il link scadrà tra 24 ore.</p>
                //     "
                // };
                // await _mailService.SendEmailAsync(mailRequest);

                return Ok(new AuthResponseModel { Status = "Success", Message = "Agente creato con successo!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore durante la creazione dell'agente: " + ex.Message });
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
        public async Task<IActionResult> Get(string? adminId, string? agencyId, string? filterRequest)
        {
            try
            {
                // Recupera l'utente corrente e il suo ruolo
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await userManager.FindByIdAsync(currentUserId);
                var userRoles = await userManager.GetRolesAsync(currentUser);
                
                //currentPage = currentPage > 0 ? currentPage : 1;
                var usersList = await userManager.GetUsersInRoleAsync("Agent");
                
                // Filtra in base al ruolo dell'utente corrente
                if (userRoles.Contains("Admin"))
                {
                    // Admin vede tutti gli agenti
                    // Se agencyId è specificato e non è 'all', filtra per quella agenzia
                    if (!string.IsNullOrEmpty(agencyId) && agencyId != "all")
                    {
                        usersList = usersList.Where(x => x.AgencyId == agencyId).ToList();
                    }
                }
                else if (userRoles.Contains("Agency"))
                {
                    // Agency vede solo i propri agenti (con AgencyId pari al proprio ID)
                    usersList = usersList.Where(x => x.AgencyId == currentUserId).ToList();
                }
                else if (userRoles.Contains("Agent"))
                {
                    // Gli Agent non possono vedere gli agenti
                    return StatusCode(403, "Accesso negato: gli agenti non possono visualizzare gli altri agenti");
                }
                else
                {
                    // Altri ruoli non dovrebbero avere accesso (già controllato da [Authorize])
                    usersList = new List<ApplicationUser>();
                }

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
                var user = await userManager.FindByIdAsync(id);
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
                ApplicationUser? user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Utente non trovato" });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route(nameof(ExportExcel))]
        //public async Task<IActionResult> ExportExcel(char? fromName, char? toName)
        //{
        //    try
        //    {
        //        var result = await _agentServices.Get(0, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<AgentSelectModel>(result.Data);
        //        byte[] fileBytes = Export.GenerateExcelContent(table);

        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Output.xlsx");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
        //    }
        //}
        //[HttpGet]
        //[Route(nameof(ExportCsv))]
        //public async Task<IActionResult> ExportCsv(char? fromName, char? toName)
        //{
        //    try
        //    {
        //        var result = await _agentServices.Get(0, null, fromName, toName);
        //        DataTable table = Export.ToDataTable<AgentSelectModel>(result.Data);
        //        byte[] fileBytes = Export.GenerateCsvContent(table);

        //        return File(fileBytes, "text/csv", "Output.csv");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });
        //    }
        //}
    }
}
