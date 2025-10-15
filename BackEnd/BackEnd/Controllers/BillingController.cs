using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.PaymentModels;
using BackEnd.Models.SubscriptionPlanModels;
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
    public class BillingController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IPaymentServices _paymentServices;
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            IStripeService stripeService,
            IPaymentServices paymentServices,
            IUserSubscriptionServices userSubscriptionServices,
            ISubscriptionPlanServices subscriptionPlanServices,
            UserManager<ApplicationUser> userManager,
            ILogger<BillingController> logger)
        {
            _stripeService = stripeService;
            _paymentServices = paymentServices;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Crea un Payment Intent per processare il pagamento di un piano
        /// </summary>
        [HttpPost("create-payment-intent")]
        [AllowAnonymous] // Permette agli utenti non ancora registrati di creare payment intent
        public async Task<ActionResult<CreatePaymentIntentResponse>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Converti l'importo in centesimi per Stripe
                long amountInCents = request.Amount;

                // Crea metadata per tracciare il piano e l'email
                var metadata = new Dictionary<string, string>
                {
                    { "plan", request.Plan },
                    { "email", request.Email ?? "" },
                    { "source", "pricing_page" }
                };

                // Crea il Payment Intent con Stripe
                var paymentIntent = await _stripeService.CreatePaymentIntentAsync(
                    amountInCents,
                    request.Currency ?? "eur",
                    request.Email ?? "",
                    metadata
                );

                // Salva il payment nel database con stato pending
                // Nota: l'utente potrebbe non esistere ancora, quindi non possiamo associare l'UserId
                // Lo associeremo quando processeremo il webhook
                var paymentModel = new PaymentCreateModel
                {
                    UserId = "", // Verrà aggiornato dopo la registrazione
                    Amount = request.Amount / 100m, // Converti da centesimi a euro
                    Currency = request.Currency ?? "EUR",
                    PaymentMethod = "stripe",
                    Status = "pending",
                    StripePaymentIntentId = paymentIntent.Id,
                    Notes = $"Piano: {request.Plan}, Email: {request.Email}"
                };

                // Non salviamo ancora il payment perché l'utente non esiste
                // Lo creeremo nel webhook quando il pagamento sarà confermato

                return Ok(new CreatePaymentIntentResponse
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del Payment Intent");
                return StatusCode(500, $"Errore durante la creazione del Payment Intent: {ex.Message}");
            }
        }

        /// <summary>
        /// Conferma un pagamento completato
        /// </summary>
        [HttpPost("confirm-payment")]
        [Authorize]
        public async Task<ActionResult<PaymentConfirmationResponse>> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Recupera il Payment Intent da Stripe
                var paymentIntent = await _stripeService.GetPaymentIntentAsync(request.PaymentIntentId);

                if (paymentIntent == null)
                    return NotFound("Payment Intent non trovato");

                // Verifica lo stato del pagamento
                if (paymentIntent.Status != "succeeded")
                {
                    return BadRequest(new PaymentConfirmationResponse
                    {
                        Success = false,
                        Message = $"Il pagamento non è stato completato. Stato: {paymentIntent.Status}"
                    });
                }

                // Recupera l'utente corrente
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Verifica se il pagamento è già stato processato
                var existingPayment = await _paymentServices.GetByStripePaymentIntentIdAsync(request.PaymentIntentId);
                if (existingPayment != null && existingPayment.Status == "completed")
                {
                    return Ok(new PaymentConfirmationResponse
                    {
                        Success = true,
                        Message = "Pagamento già processato",
                        PaymentId = existingPayment.Id
                    });
                }

                // Il pagamento verrà processato completamente dal webhook
                // Qui restituiamo solo una conferma
                return Ok(new PaymentConfirmationResponse
                {
                    Success = true,
                    Message = "Pagamento in elaborazione",
                    PaymentId = existingPayment?.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la conferma del pagamento");
                return StatusCode(500, $"Errore durante la conferma del pagamento: {ex.Message}");
            }
        }

        /// <summary>
        /// Recupera i piani di abbonamento disponibili
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanSelectModel>>> GetPlans()
        {
            try
            {
                var plans = await _subscriptionPlanServices.GetActivePlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei piani");
                return StatusCode(500, $"Errore durante il recupero dei piani: {ex.Message}");
            }
        }

        /// <summary>
        /// Recupera lo stato dell'abbonamento corrente dell'utente
        /// </summary>
        [HttpGet("subscription-status")]
        [Authorize]
        public async Task<ActionResult<UserSubscriptionSelectModel>> GetSubscriptionStatus()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId);
                
                if (subscription == null)
                    return NotFound("Nessun abbonamento attivo trovato");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato dell'abbonamento");
                return StatusCode(500, $"Errore durante il recupero dello stato dell'abbonamento: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancella l'abbonamento corrente
        /// </summary>
        [HttpPost("cancel-subscription")]
        [Authorize]
        public async Task<ActionResult> CancelSubscription()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId);
                
                if (subscription == null)
                    return NotFound("Nessun abbonamento attivo trovato");

                // Cancella la subscription su Stripe se presente
                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
                }

                // Cancella la subscription nel database
                await _userSubscriptionServices.CancelSubscriptionAsync(subscription.Id);

                return Ok("Abbonamento cancellato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la cancellazione dell'abbonamento");
                return StatusCode(500, $"Errore durante la cancellazione dell'abbonamento: {ex.Message}");
            }
        }
    }

    // DTOs
    public class CreatePaymentIntentRequest
    {
        public string Plan { get; set; } = null!;
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? Email { get; set; }
    }

    public class CreatePaymentIntentResponse
    {
        public string ClientSecret { get; set; } = null!;
        public string PaymentIntentId { get; set; } = null!;
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = null!;
    }

    public class PaymentConfirmationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? PaymentId { get; set; }
    }
}

