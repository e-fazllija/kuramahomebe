using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionLimitModels;
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
    public class SubscriptionLimitController : ControllerBase
    {
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionLimitController(
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager)
        {
            _subscriptionLimitService = subscriptionLimitService;
            _userManager = userManager;
        }

        /// <summary>
        /// Verifica il limite per una feature specifica
        /// </summary>
        /// <param name="featureName">Nome della feature (es: "max_agencies", "max_properties")</param>
        /// <returns>Status del limite</returns>
        [HttpGet("check")]
        public async Task<ActionResult<SubscriptionLimitStatusResponse>> CheckFeatureLimit(
            [FromQuery] string featureName)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Utente non autorizzato");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("Utente non trovato");

                var result = await _subscriptionLimitService.CheckFeatureLimitAsync(
                    userId, 
                    featureName, 
                    user.AdminId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        /// <summary>
        /// Recupera tutti i limiti dello stato per l'utente corrente
        /// </summary>
        /// <returns>Dictionary con tutti i limiti (chiave = featureName)</returns>
        [HttpGet("my-status")]
        public async Task<ActionResult<Dictionary<string, SubscriptionLimitStatusResponse>>> GetMyLimitsStatus()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Utente non autorizzato");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("Utente non trovato");

                var limits = await _subscriptionLimitService.GetAllLimitsAsync(userId, user.AdminId);

                return Ok(limits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se il downgrade al piano specificato è possibile
        /// </summary>
        /// <param name="planId">ID del piano di destinazione</param>
        /// <returns>Response con compatibilità e dettagli delle features</returns>
        [HttpGet("check-downgrade")]
        public async Task<ActionResult<DowngradeCompatibilityResponse>> CheckDowngradeCompatibility(
            [FromQuery] int planId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Utente non autorizzato");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("Utente non trovato");

                var result = await _subscriptionLimitService.CheckDowngradeCompatibilityAsync(
                    userId,
                    planId,
                    user.AdminId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}

