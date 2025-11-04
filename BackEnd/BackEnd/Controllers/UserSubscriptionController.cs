using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.UserSubscriptionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Entities;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSubscriptionController : ControllerBase
    {
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSubscriptionController(IUserSubscriptionServices userSubscriptionServices, UserManager<ApplicationUser> userManager)
        {
            _userSubscriptionServices = userSubscriptionServices;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserSubscriptionSelectModel>> GetById(int id)
        {
            try
            {
                var subscription = await _userSubscriptionServices.GetByIdAsync(id);
                if (subscription == null)
                    return NotFound($"Abbonamento con ID {id} non trovato");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserSubscriptionSelectModel>>> GetUserSubscriptions(string userId)
        {
            try
            {
                var subscriptions = await _userSubscriptionServices.GetUserSubscriptionsAsync(userId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<UserSubscriptionSelectModel>> GetActiveUserSubscription(string userId)
        {
            try
            {
                // Recupera l'utente per ottenere AgencyId per l'ereditarietà
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound($"Utente con ID {userId} non trovato");

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (subscription == null)
                    return NotFound($"Nessun abbonamento attivo trovato per l'utente {userId}");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("stripe/{stripeSubscriptionId}")]
        public async Task<ActionResult<UserSubscriptionSelectModel>> GetByStripeSubscriptionId(string stripeSubscriptionId)
        {
            try
            {
                var subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
                if (subscription == null)
                    return NotFound($"Abbonamento con Stripe ID {stripeSubscriptionId} non trovato");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<UserSubscriptionSelectModel>>> GetExpiredSubscriptions()
        {
            try
            {
                var subscriptions = await _userSubscriptionServices.GetExpiredSubscriptionsAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserSubscriptionSelectModel>> Create([FromBody] UserSubscriptionCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var subscription = await _userSubscriptionServices.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserSubscriptionSelectModel>> Update(int id, [FromBody] UserSubscriptionUpdateModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest("L'ID nell'URL non corrisponde all'ID nel modello");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var subscription = await _userSubscriptionServices.UpdateAsync(model);
                if (subscription == null)
                    return NotFound($"Abbonamento con ID {id} non trovato");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _userSubscriptionServices.DeleteAsync(id);
                if (!result)
                    return NotFound($"Abbonamento con ID {id} non trovato");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelSubscription(int id)
        {
            try
            {
                var result = await _userSubscriptionServices.CancelSubscriptionAsync(id);
                if (!result)
                    return NotFound($"Abbonamento con ID {id} non trovato");

                return Ok("Abbonamento cancellato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("{id}/renew")]
        public async Task<ActionResult> RenewSubscription(int id)
        {
            try
            {
                var result = await _userSubscriptionServices.RenewSubscriptionAsync(id);
                if (!result)
                    return NotFound($"Abbonamento con ID {id} non trovato");

                return Ok("Abbonamento rinnovato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/has-active")]
        public async Task<ActionResult<bool>> HasActiveSubscription(string userId)
        {
            try
            {
                var hasActive = await _userSubscriptionServices.HasActiveSubscriptionAsync(userId);
                return Ok(hasActive);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method per verificare se l'utente è Admin
        /// </summary>
        private async Task<bool> IsAdminAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Admin");
        }
    }
}
