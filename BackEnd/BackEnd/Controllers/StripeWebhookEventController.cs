using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.StripeWebhookEventModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StripeWebhookEventController : ControllerBase
    {
        private readonly IStripeWebhookEventServices _stripeWebhookEventServices;

        public StripeWebhookEventController(IStripeWebhookEventServices stripeWebhookEventServices)
        {
            _stripeWebhookEventServices = stripeWebhookEventServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StripeWebhookEventSelectModel>>> GetAll()
        {
            try
            {
                var events = await _stripeWebhookEventServices.GetAllAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StripeWebhookEventSelectModel>> GetById(int id)
        {
            try
            {
                var webhookEvent = await _stripeWebhookEventServices.GetByIdAsync(id);
                if (webhookEvent == null)
                    return NotFound($"Evento webhook con ID {id} non trovato");

                return Ok(webhookEvent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<StripeWebhookEventSelectModel>> GetByEventId(string eventId)
        {
            try
            {
                var webhookEvent = await _stripeWebhookEventServices.GetByEventIdAsync(eventId);
                if (webhookEvent == null)
                    return NotFound($"Evento webhook con Event ID {eventId} non trovato");

                return Ok(webhookEvent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("unprocessed")]
        public async Task<ActionResult<IEnumerable<StripeWebhookEventSelectModel>>> GetUnprocessedEvents()
        {
            try
            {
                var events = await _stripeWebhookEventServices.GetUnprocessedEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("processed/{eventId}")]
        public async Task<ActionResult<bool>> IsEventProcessed(string eventId)
        {
            try
            {
                var isProcessed = await _stripeWebhookEventServices.IsEventProcessedAsync(eventId);
                return Ok(isProcessed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StripeWebhookEventSelectModel>> Create([FromBody] StripeWebhookEventCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var webhookEvent = await _stripeWebhookEventServices.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = webhookEvent.Id }, webhookEvent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StripeWebhookEventSelectModel>> Update(int id, [FromBody] StripeWebhookEventUpdateModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest("L'ID nell'URL non corrisponde all'ID nel modello");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var webhookEvent = await _stripeWebhookEventServices.UpdateAsync(model);
                if (webhookEvent == null)
                    return NotFound($"Evento webhook con ID {id} non trovato");

                return Ok(webhookEvent);
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
                var result = await _stripeWebhookEventServices.DeleteAsync(id);
                if (!result)
                    return NotFound($"Evento webhook con ID {id} non trovato");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("{id}/mark-processed")]
        public async Task<ActionResult> MarkAsProcessed(int id)
        {
            try
            {
                var result = await _stripeWebhookEventServices.MarkAsProcessedAsync(id);
                if (!result)
                    return NotFound($"Evento webhook con ID {id} non trovato");

                return Ok("Evento webhook marcato come processato");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("process")]
        public async Task<ActionResult> ProcessWebhookEvent([FromBody] ProcessWebhookEventModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _stripeWebhookEventServices.ProcessWebhookEventAsync(model.EventId, model.EventType, model.Data);
                if (!result)
                    return BadRequest("Errore durante il processing dell'evento webhook");

                return Ok("Evento webhook processato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }

    public class ProcessWebhookEventModel
    {
        public string EventId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Data { get; set; } = null!;
    }
}
