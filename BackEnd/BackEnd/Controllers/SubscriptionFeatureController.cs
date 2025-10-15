using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.SubscriptionFeatureModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionFeatureController : ControllerBase
    {
        private readonly ISubscriptionFeatureServices _subscriptionFeatureServices;

        public SubscriptionFeatureController(ISubscriptionFeatureServices subscriptionFeatureServices)
        {
            _subscriptionFeatureServices = subscriptionFeatureServices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionFeatureSelectModel>> GetById(int id)
        {
            try
            {
                var feature = await _subscriptionFeatureServices.GetByIdAsync(id);
                if (feature == null)
                    return NotFound($"Funzionalità con ID {id} non trovata");

                return Ok(feature);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("plan/{planId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionFeatureSelectModel>>> GetByPlanId(int planId)
        {
            try
            {
                var features = await _subscriptionFeatureServices.GetByPlanIdAsync(planId);
                return Ok(features);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionFeatureSelectModel>> Create([FromBody] SubscriptionFeatureCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var feature = await _subscriptionFeatureServices.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = feature.Id }, feature);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("multiple")]
        public async Task<ActionResult<IEnumerable<SubscriptionFeatureSelectModel>>> CreateMultiple([FromBody] IEnumerable<SubscriptionFeatureCreateModel> models)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var features = await _subscriptionFeatureServices.CreateMultipleAsync(models);
                return Ok(features);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionFeatureSelectModel>> Update(int id, [FromBody] SubscriptionFeatureUpdateModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest("L'ID nell'URL non corrisponde all'ID nel modello");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var feature = await _subscriptionFeatureServices.UpdateAsync(model);
                if (feature == null)
                    return NotFound($"Funzionalità con ID {id} non trovata");

                return Ok(feature);
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
                var result = await _subscriptionFeatureServices.DeleteAsync(id);
                if (!result)
                    return NotFound($"Funzionalità con ID {id} non trovata");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpDelete("plan/{planId}")]
        public async Task<ActionResult> DeleteByPlanId(int planId)
        {
            try
            {
                var result = await _subscriptionFeatureServices.DeleteByPlanIdAsync(planId);
                if (!result)
                    return NotFound($"Nessuna funzionalità trovata per il piano con ID {planId}");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
