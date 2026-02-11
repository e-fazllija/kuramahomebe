using BackEnd.Helpers;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [Authorize(Policy = "ActiveSubscription")]
    [ApiController]
    [Route("/api/[controller]/")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSubscriptionServices _userSubscriptionServices;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger,
            UserManager<ApplicationUser> userManager,
            IUserSubscriptionServices userSubscriptionServices)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _userManager = userManager;
            _userSubscriptionServices = userSubscriptionServices;
        }

        /// <summary>
        /// Recupera i dati necessari per il widget mappa (Widget13)
        /// </summary>
        /// <param name="agencyId">ID dell'agenzia o agente per filtrare (formato: "agency_xxx" o "agent_xxx" o "all")</param>
        /// <param name="year">Anno per filtrare i dati (opzionale, default anno corrente)</param>
        /// <returns>Dati della mappa con totali KPI, agenzie e agenti</returns>
        [HttpGet]
        [Route(nameof(GetMapData))]
        public async Task<IActionResult> GetMapData(string? agencyId = null, int? year = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                var result = await _dashboardService.GetMapData(userId, agencyId, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetMapData");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati necessari per il widget grafici immobili (Widget3)
        /// Admin e Agency solo con piano Pro o Premium
        /// </summary>
        /// <param name="agencyId">ID dell'agenzia o agente per filtrare (formato: "agency_xxx" o "agent_xxx" o "all")</param>
        /// <param name="year">Anno per filtrare i dati (opzionale, default anno corrente)</param>
        /// <returns>Dati per i grafici con immobili inseriti/venduti e provvigioni</returns>
        [HttpGet]
        [Route(nameof(GetWidget3Data))]
        public async Task<IActionResult> GetWidget3Data(string? agencyId = null, int? year = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Pro o Premium
                if (!await CanAccessWidget3Async(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: Admin e Agency possono accedere solo con piano Pro o Premium" });
                }

                var result = await _dashboardService.GetWidget3Data(userId, agencyId, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetWidget3Data");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati delle Top Agenzie per Widget7
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <param name="year">Anno per filtrare i dati (opzionale, default anno corrente)</param>
        /// <param name="sortBy">Campo per ordinare (properties, customers, requests, soldProperties, appointments, commissions)</param>
        /// <param name="sortOrder">Ordine di ordinamento (asc o desc, default desc)</param>
        /// <returns>Dati aggregati delle agenzie con statistiche (Top 5)</returns>
        [HttpGet]
        [Route(nameof(GetTopAgenciesData))]
        public async Task<IActionResult> GetTopAgenciesData(int? year = null, string? sortBy = null, string? sortOrder = "desc")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetTopAgenciesData(userId, year, sortBy, sortOrder);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetTopAgenciesData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati dei Top Agenti per Widget7
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <param name="year">Anno per filtrare i dati (opzionale, default anno corrente)</param>
        /// <param name="sortBy">Campo per ordinare (soldProperties, loadedProperties, requests, appointments, commissions)</param>
        /// <param name="sortOrder">Ordine di ordinamento (asc o desc, default desc)</param>
        /// <returns>Dati aggregati degli agenti con statistiche (Top 5)</returns>
        [HttpGet]
        [Route(nameof(GetTopAgentsData))]
        public async Task<IActionResult> GetTopAgentsData(int? year = null, string? sortBy = null, string? sortOrder = "desc")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetTopAgentsData(userId, year, sortBy, sortOrder);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetTopAgentsData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati delle Top Zone per Widget7
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <returns>Dati aggregati delle zone (Top 5 per immobili e richieste)</returns>
        [HttpGet]
        [Route(nameof(GetTopZonesData))]
        public async Task<IActionResult> GetTopZonesData()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetTopZonesData(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetTopZonesData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati delle Top Tipologie per Widget7
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <returns>Dati aggregati delle tipologie (Top 5 per immobili e richieste)</returns>
        [HttpGet]
        [Route(nameof(GetTopTypologiesData))]
        public async Task<IActionResult> GetTopTypologiesData()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetTopTypologiesData(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetTopTypologiesData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati Top Guadagni (portafoglio e vendite anno) per Widget7
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <param name="year">Anno per filtrare le vendite</param>
        [HttpGet]
        [Route(nameof(GetTopEarningsData))]
        public async Task<IActionResult> GetTopEarningsData(int? year = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetTopEarningsData(userId, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetTopEarningsData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera i dati Analytics per Widget11 (richieste, immobili, clienti, appuntamenti)
        /// </summary>
        /// <param name="year">Anno per filtrare i dati (obbligatorio)</param>
        /// <param name="agencyId">ID dell'agenzia o agente per filtrare (opzionale, formato: "agency_xxx" o "agent_xxx")</param>
        /// <returns>Dati analytics mensili per tutte le categorie</returns>
        [HttpGet]
        [Route(nameof(GetAnalyticsData))]
        public async Task<IActionResult> GetAnalyticsData(int year, string? agencyId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetAnalyticsData(userId, year, agencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetAnalyticsData: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera le richieste matchate ordinate per percentuale match
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <returns>Lista di richieste matchate con miglior immobile e percentuale match</returns>
        [HttpGet]
        [Route(nameof(GetMatchedRequests))]
        public async Task<IActionResult> GetMatchedRequests()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetMatchedRequests(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetMatchedRequests: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Recupera gli immobili con incarico in scadenza (meno di 30 giorni)
        /// Admin e Agency con piano Premium possono accedere
        /// </summary>
        /// <param name="daysThreshold">Soglia in giorni per considerare un incarico in scadenza (default: 30)</param>
        /// <returns>Lista di immobili con incarico in scadenza, ordinati per scadenza più imminente</returns>
        [HttpGet]
        [Route(nameof(GetExpiringAssignments))]
        public async Task<IActionResult> GetExpiringAssignments(int? daysThreshold = 30)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Utente non autenticato" });
                }

                // Verifica che l'utente sia Admin o Agency con piano Premium
                if (!await IsAdminOrAgencyPremiumAsync(userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, 
                        new AuthResponseModel { Status = "Error", Message = "Accesso negato: solo Admin o Agency con piano Premium può accedere a questa funzionalità" });
                }

                var result = await _dashboardService.GetExpiringAssignments(userId, daysThreshold);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in GetExpiringAssignments: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseModel { Status = "Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se l'utente è Admin con piano Premium
        /// </summary>
        private async Task<bool> IsAdminPremiumAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin"))
                    return false;

                return await _userSubscriptionServices.HasPremiumPlanAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in IsAdminPremiumAsync per userId {userId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se l'utente è Admin O Agency con piano Premium
        /// </summary>
        private async Task<bool> IsAdminOrAgencyPremiumAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var roles = await _userManager.GetRolesAsync(user);
                // Verifica se è Admin o Agency
                if (!roles.Contains("Admin") && !roles.Contains("Agency"))
                    return false;

                // Per le Agency, passa l'AdminId per permettere la risalita all'abbonamento dell'Admin
                var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (activeSubscription == null) return false;
                
                var planName = activeSubscription.SubscriptionPlan?.Name;
                var status = activeSubscription.Status?.ToLowerInvariant() ?? "";
                // Includi anche piani prepagati (es. "Premium 3 Months", "Premium 12 Months")
                return SubscriptionPlanTierHelper.IsPremiumPlanName(planName) && status == "active";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in IsAdminOrAgencyPremiumAsync per userId {userId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se l'utente può accedere a Widget3 (Admin e Agency solo con Pro o Premium)
        /// </summary>
        private async Task<bool> CanAccessWidget3Async(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var roles = await _userManager.GetRolesAsync(user);
                
                // Verifica se è Admin o Agency
                if (!roles.Contains("Admin") && !roles.Contains("Agency"))
                    return false;
                
                // Per le Agency, passa l'AdminId per permettere la risalita all'abbonamento dell'Admin
                // Per gli Admin, passa null
                var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (activeSubscription == null) return false;
                
                var planName = activeSubscription.SubscriptionPlan?.Name;
                var status = activeSubscription.Status?.ToLowerInvariant() ?? "";
                // Includi anche piani prepagati (es. "Pro 6 Months", "Premium 12 Months")
                return SubscriptionPlanTierHelper.IsProOrPremiumPlanName(planName) && status == "active";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in CanAccessWidget3Async per userId {userId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se l'utente è Admin O Agency con piano Pro o Premium
        /// </summary>
        private async Task<bool> IsAdminOrAgencyProOrPremiumAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var roles = await _userManager.GetRolesAsync(user);
                // Verifica se è Admin o Agency
                if (!roles.Contains("Admin") && !roles.Contains("Agency"))
                    return false;

                // Per le Agency, passa l'AdminId per permettere la risalita all'abbonamento dell'Admin
                var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (activeSubscription == null) return false;
                
                var planName = activeSubscription.SubscriptionPlan?.Name;
                var status = activeSubscription.Status?.ToLowerInvariant() ?? "";
                // Includi anche piani prepagati (es. "Pro 6 Months", "Premium 12 Months")
                return SubscriptionPlanTierHelper.IsProOrPremiumPlanName(planName) && status == "active";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore in IsAdminOrAgencyProOrPremiumAsync per userId {userId}: {ex.Message}");
                return false;
            }
        }
    }
}


