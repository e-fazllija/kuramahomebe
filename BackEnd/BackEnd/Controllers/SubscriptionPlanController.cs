using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionPlanModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BackEnd.Entities;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionPlanController(ISubscriptionPlanServices subscriptionPlanServices, UserManager<ApplicationUser> userManager)
        {
            _subscriptionPlanServices = subscriptionPlanServices;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanSelectModel>>> GetAll()
        {
            try
            {
                // Controllo: solo Admin può vedere tutti i piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono visualizzare tutti i piani di abbonamento");
                }

                var plans = await _subscriptionPlanServices.GetAllAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanSelectModel>>> GetActivePlans()
        {
            try
            {
                // Controllo: solo Admin può vedere i piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono visualizzare i piani di abbonamento");
                }

                var plans = await _subscriptionPlanServices.GetActivePlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionPlanSelectModel>> GetById(int id)
        {
            try
            {
                // Controllo: solo Admin può vedere i dettagli dei piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono visualizzare i dettagli dei piani di abbonamento");
                }

                var plan = await _subscriptionPlanServices.GetByIdAsync(id);
                if (plan == null)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("{id}/with-features")]
        public async Task<ActionResult<SubscriptionPlanSelectModel>> GetWithFeatures(int id)
        {
            try
            {
                // Controllo: solo Admin può vedere i piani con le loro features
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono visualizzare i piani con le loro features");
                }

                var plan = await _subscriptionPlanServices.GetPlanWithFeaturesAsync(id);
                if (plan == null)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionPlanSelectModel>> Create([FromBody] SubscriptionPlanCreateModel model)
        {
            try
            {
                // Controllo: solo Admin può creare piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono creare piani di abbonamento");
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Verifica unicità del nome
                if (!await _subscriptionPlanServices.IsNameUniqueAsync(model.Name))
                    return BadRequest("Esiste già un piano di abbonamento con questo nome");

                var plan = await _subscriptionPlanServices.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionPlanSelectModel>> Update(int id, [FromBody] SubscriptionPlanUpdateModel model)
        {
            try
            {
                // Controllo: solo Admin può modificare piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono modificare piani di abbonamento");
                }

                if (id != model.Id)
                    return BadRequest("L'ID nell'URL non corrisponde all'ID nel modello");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Verifica unicità del nome
                if (!await _subscriptionPlanServices.IsNameUniqueAsync(model.Name, id))
                    return BadRequest("Esiste già un piano di abbonamento con questo nome");

                var plan = await _subscriptionPlanServices.UpdateAsync(model);
                if (plan == null)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return Ok(plan);
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
                // Controllo: solo Admin può eliminare piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono eliminare piani di abbonamento");
                }

                var result = await _subscriptionPlanServices.DeleteAsync(id);
                if (!result)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                // Controllo: solo Admin può attivare piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono attivare piani di abbonamento");
                }

                var result = await _subscriptionPlanServices.ActivateAsync(id);
                if (!result)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return Ok("Piano di abbonamento attivato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                // Controllo: solo Admin può disattivare piani di abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono disattivare piani di abbonamento");
                }

                var result = await _subscriptionPlanServices.DeactivateAsync(id);
                if (!result)
                    return NotFound($"Piano di abbonamento con ID {id} non trovato");

                return Ok("Piano di abbonamento disattivato con successo");
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
