using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionPlanModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;

        public SubscriptionPlanController(ISubscriptionPlanServices subscriptionPlanServices)
        {
            _subscriptionPlanServices = subscriptionPlanServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanSelectModel>>> GetAll()
        {
            try
            {
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
    }
}
