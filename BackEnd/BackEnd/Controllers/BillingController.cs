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

                decimal originalAmount = request.Amount / 100m; // Importo originale in euro
                decimal creditAmount = 0;
                decimal finalAmount = originalAmount;
                bool isUpgrade = false;
                string? currentPlanId = null;
                string? currentPlanName = null;

                // Se l'email è fornita, prova a recuperare l'utente e verificare se è un upgrade
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user != null)
                    {
                        // Recupera abbonamento corrente
                        var currentSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);
                        
                        if (currentSubscription != null && currentSubscription.SubscriptionPlan != null)
                        {
                            // Recupera il nuovo piano
                            var allPlans = await _subscriptionPlanServices.GetActivePlansAsync();
                            var newPlan = allPlans.FirstOrDefault(p => 
                                p.Name.Equals(request.Plan, StringComparison.OrdinalIgnoreCase));

                            if (newPlan != null)
                            {
                                var currentPlanPrice = currentSubscription.SubscriptionPlan.Price;
                                var newPlanPrice = newPlan.Price;

                                // Verifica se è un upgrade
                                isUpgrade = newPlanPrice > currentPlanPrice && 
                                           currentSubscription.SubscriptionPlanId != newPlan.Id;

                                if (isUpgrade)
                                {
                                    // Verifica se l'abbonamento è scaduto
                                    var today = DateTime.UtcNow;
                                    var isExpired = !currentSubscription.EndDate.HasValue || 
                                                   currentSubscription.EndDate.Value <= today;

                                    if (!isExpired)
                                    {
                                        // Calcola credito residuo
                                        var endDate = currentSubscription.EndDate.Value;
                                        var startDate = currentSubscription.StartDate;

                                        // Durata ciclo attuale (in giorni)
                                        var cycleDuration = (endDate - startDate).TotalDays;
                                        if (cycleDuration <= 0)
                                            cycleDuration = 30; // Default a 30 giorni

                                        // Giorni rimasti
                                        var daysRemaining = (endDate - today).TotalDays;
                                        if (daysRemaining < 0)
                                            daysRemaining = 0;

                                        // Calcolo credito proporzionale
                                        var dailyRate = currentPlanPrice / (decimal)cycleDuration;
                                        creditAmount = dailyRate * (decimal)daysRemaining;
                                        creditAmount = Math.Round(creditAmount, 2, MidpointRounding.AwayFromZero);

                                        // Importo netto da pagare
                                        finalAmount = Math.Max(0, newPlanPrice - creditAmount);
                                        finalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero);

                                        currentPlanId = currentSubscription.SubscriptionPlanId.ToString();
                                        currentPlanName = currentSubscription.SubscriptionPlan.Name;

                                        _logger.LogInformation(
                                            "Upgrade rilevato per utente {UserId} ({Email}). " +
                                            "Piano corrente: {CurrentPlan} (€{CurrentPrice}), " +
                                            "Nuovo piano: {NewPlan} (€{NewPrice}), " +
                                            "Giorni rimasti: {DaysRemaining}, " +
                                            "Credito: €{Credit}, Importo finale: €{FinalAmount}",
                                            user.Id, request.Email, currentPlanName, currentPlanPrice,
                                            newPlan.Name, newPlanPrice, (int)daysRemaining,
                                            creditAmount, finalAmount);
                                    }
                                    else
                                    {
                                        _logger.LogInformation(
                                            "Upgrade rilevato per utente {UserId} ({Email}) ma abbonamento scaduto. " +
                                            "Nessun credito applicato.",
                                            user.Id, request.Email);
                                    }
                                }
                            }
                        }
                    }
                }

                // Converti l'importo finale in centesimi per Stripe
                long amountInCents = (long)(finalAmount * 100);

                // Crea metadata per tracciare il piano, l'email e le info di upgrade
                var metadata = new Dictionary<string, string>
                {
                    { "plan", request.Plan },
                    { "email", request.Email ?? "" },
                    { "source", "pricing_page" },
                    { "isUpgrade", isUpgrade.ToString().ToLower() },
                    { "originalAmount", originalAmount.ToString("F2") },
                    { "creditAmount", creditAmount.ToString("F2") },
                    { "finalAmount", finalAmount.ToString("F2") }
                };

                if (!string.IsNullOrEmpty(currentPlanId))
                {
                    metadata["currentPlanId"] = currentPlanId;
                    metadata["currentPlanName"] = currentPlanName ?? "";
                }

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
                    Amount = finalAmount, // Usa l'importo finale calcolato
                    Currency = request.Currency ?? "EUR",
                    PaymentMethod = "stripe",
                    Status = "pending",
                    StripePaymentIntentId = paymentIntent.Id,
                    Notes = isUpgrade 
                        ? $"Upgrade piano: {request.Plan}, Credito applicato: €{creditAmount:F2}, Importo originale: €{originalAmount:F2}, Importo finale: €{finalAmount:F2}"
                        : $"Piano: {request.Plan}, Email: {request.Email}"
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
        /// Calcola il credito residuo e l'importo netto per un upgrade
        /// </summary>
        [HttpGet("calculate-upgrade-credit")]
        [Authorize]
        public async Task<ActionResult<UpgradeCreditCalculationResponse>> CalculateUpgradeCredit([FromQuery] string planName)
        {
            try
            {
                if (string.IsNullOrEmpty(planName))
                    return BadRequest("Il nome del piano è obbligatorio");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                // Recupera abbonamento corrente
                var currentSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                
                // Se non ha abbonamento attivo, non c'è credito da calcolare
                if (currentSubscription == null)
                {
                    return Ok(new UpgradeCreditCalculationResponse
                    {
                        IsUpgrade = false,
                        HasActiveSubscription = false,
                        CreditAmount = 0,
                        OriginalAmount = 0,
                        FinalAmount = 0,
                        DaysRemaining = 0,
                        Message = "Nessun abbonamento attivo trovato"
                    });
                }

                // Recupera il nuovo piano
                var allPlans = await _subscriptionPlanServices.GetActivePlansAsync();
                var newPlan = allPlans.FirstOrDefault(p => 
                    p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));

                if (newPlan == null)
                    return NotFound($"Piano '{planName}' non trovato");

                var currentPlan = currentSubscription.SubscriptionPlan;
                if (currentPlan == null)
                    return BadRequest("Impossibile recuperare i dettagli del piano corrente");

                var currentPlanPrice = currentPlan.Price;
                var newPlanPrice = newPlan.Price;

                // Verifica se è un upgrade
                bool isUpgrade = newPlanPrice > currentPlanPrice;
                bool isSamePlan = currentSubscription.SubscriptionPlanId == newPlan.Id;

                // Se è lo stesso piano o un downgrade, non c'è credito
                if (!isUpgrade || isSamePlan)
                {
                    return Ok(new UpgradeCreditCalculationResponse
                    {
                        IsUpgrade = false,
                        HasActiveSubscription = true,
                        CreditAmount = 0,
                        OriginalAmount = newPlanPrice,
                        FinalAmount = newPlanPrice,
                        DaysRemaining = 0,
                        Message = isSamePlan ? "Stesso piano selezionato" : "Non è un upgrade"
                    });
                }

                // Verifica se l'abbonamento è scaduto
                var today = DateTime.UtcNow;
                var isExpired = !currentSubscription.EndDate.HasValue || 
                               currentSubscription.EndDate.Value <= today;

                // Se scaduto, non c'è credito
                if (isExpired)
                {
                    return Ok(new UpgradeCreditCalculationResponse
                    {
                        IsUpgrade = true,
                        HasActiveSubscription = true,
                        CreditAmount = 0,
                        OriginalAmount = newPlanPrice,
                        FinalAmount = newPlanPrice,
                        DaysRemaining = 0,
                        Message = "Abbonamento scaduto, nessun credito residuo"
                    });
                }

                // Calcola credito residuo
                var endDate = currentSubscription.EndDate.Value;
                var startDate = currentSubscription.StartDate;

                // Durata ciclo attuale (in giorni)
                var cycleDuration = (endDate - startDate).TotalDays;
                if (cycleDuration <= 0)
                    cycleDuration = 30; // Default a 30 giorni se calcolo anomalo

                // Giorni rimasti
                var daysRemaining = (endDate - today).TotalDays;
                if (daysRemaining < 0)
                    daysRemaining = 0;

                // Calcolo credito proporzionale (converti double a decimal per operazioni con decimal)
                var dailyRate = currentPlanPrice / (decimal)cycleDuration;
                var creditAmount = dailyRate * (decimal)daysRemaining;
                creditAmount = Math.Round(creditAmount, 2, MidpointRounding.AwayFromZero);

                // Importo netto da pagare
                var finalAmount = Math.Max(0, newPlanPrice - creditAmount);
                finalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero);

                _logger.LogInformation(
                    "Calcolo credito upgrade per utente {UserId}. Piano corrente: {CurrentPlan} (€{CurrentPrice}), " +
                    "Nuovo piano: {NewPlan} (€{NewPrice}), Giorni rimasti: {DaysRemaining}, " +
                    "Credito: €{Credit}, Importo finale: €{FinalAmount}",
                    userId, currentPlan.Name, currentPlanPrice, newPlan.Name, newPlanPrice,
                    (int)daysRemaining, creditAmount, finalAmount);

                return Ok(new UpgradeCreditCalculationResponse
                {
                    IsUpgrade = true,
                    HasActiveSubscription = true,
                    CreditAmount = creditAmount,
                    OriginalAmount = newPlanPrice,
                    FinalAmount = finalAmount,
                    DaysRemaining = (int)daysRemaining,
                    CurrentPlanName = currentPlan.Name,
                    NewPlanName = newPlan.Name,
                    Message = "Calcolo completato con successo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il calcolo del credito upgrade");
                return StatusCode(500, $"Errore durante il calcolo del credito: {ex.Message}");
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
                // Controllo: solo Admin può vedere lo stato dell'abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono visualizzare lo stato dell'abbonamento");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Recupera l'utente per ottenere AgencyId
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                
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
                // Controllo: solo Admin può cancellare l'abbonamento
                if (!await IsAdminAsync())
                {
                    return StatusCode(403, "Accesso negato: solo gli Admin possono cancellare l'abbonamento");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Recupera l'utente per ottenere AgencyId
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                
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

    public class UpgradeCreditCalculationResponse
    {
        public bool IsUpgrade { get; set; }
        public bool HasActiveSubscription { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int DaysRemaining { get; set; }
        public string? CurrentPlanName { get; set; }
        public string? NewPlanName { get; set; }
        public string Message { get; set; } = null!;
    }
}

