using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.PaymentModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentServices _paymentServices;

        public PaymentController(IPaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentSelectModel>> GetById(int id)
        {
            try
            {
                var payment = await _paymentServices.GetByIdAsync(id);
                if (payment == null)
                    return NotFound($"Pagamento con ID {id} non trovato");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PaymentSelectModel>>> GetUserPayments(string userId)
        {
            try
            {
                var payments = await _paymentServices.GetUserPaymentsAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("subscription/{subscriptionId}")]
        public async Task<ActionResult<IEnumerable<PaymentSelectModel>>> GetPaymentsBySubscriptionId(int subscriptionId)
        {
            try
            {
                var payments = await _paymentServices.GetPaymentsBySubscriptionIdAsync(subscriptionId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("stripe/{paymentIntentId}")]
        public async Task<ActionResult<PaymentSelectModel>> GetByStripePaymentIntentId(string paymentIntentId)
        {
            try
            {
                var payment = await _paymentServices.GetByStripePaymentIntentIdAsync(paymentIntentId);
                if (payment == null)
                    return NotFound($"Pagamento con Stripe PaymentIntent ID {paymentIntentId} non trovato");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<ActionResult<PaymentSelectModel>> GetByTransactionId(string transactionId)
        {
            try
            {
                var payment = await _paymentServices.GetByTransactionIdAsync(transactionId);
                if (payment == null)
                    return NotFound($"Pagamento con Transaction ID {transactionId} non trovato");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<PaymentSelectModel>>> GetPaymentsByStatus(string status)
        {
            try
            {
                var payments = await _paymentServices.GetPaymentsByStatusAsync(status);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("revenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var revenue = await _paymentServices.GetTotalRevenueAsync(fromDate, toDate);
                return Ok(revenue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentSelectModel>> Create([FromBody] PaymentCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var payment = await _paymentServices.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentSelectModel>> Update(int id, [FromBody] PaymentUpdateModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest("L'ID nell'URL non corrisponde all'ID nel modello");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var payment = await _paymentServices.UpdateAsync(model);
                if (payment == null)
                    return NotFound($"Pagamento con ID {id} non trovato");

                return Ok(payment);
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
                var result = await _paymentServices.DeleteAsync(id);
                if (!result)
                    return NotFound($"Pagamento con ID {id} non trovato");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdatePaymentStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _paymentServices.UpdatePaymentStatusAsync(id, status);
                if (!result)
                    return NotFound($"Pagamento con ID {id} non trovato");

                return Ok("Stato del pagamento aggiornato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("stripe/process")]
        public async Task<ActionResult> ProcessStripePayment([FromBody] ProcessStripePaymentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _paymentServices.ProcessStripePaymentAsync(model.PaymentIntentId, model.Status);
                if (!result)
                    return NotFound($"Pagamento con PaymentIntent ID {model.PaymentIntentId} non trovato");

                return Ok("Pagamento Stripe processato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }

    public class ProcessStripePaymentModel
    {
        public string PaymentIntentId { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
