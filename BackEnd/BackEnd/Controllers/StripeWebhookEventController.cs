using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.StripeWebhookEventModels;
using BackEnd.Models.PaymentModels;
using BackEnd.Models.UserSubscriptionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BackEnd.Entities;
using Stripe;
using System.Text.Json;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StripeWebhookEventController : ControllerBase
    {
        private readonly IStripeWebhookEventServices _stripeWebhookEventServices;
        private readonly IStripeService _stripeService;
        private readonly IPaymentServices _paymentServices;
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StripeWebhookEventController> _logger;

        public StripeWebhookEventController(
            IStripeWebhookEventServices stripeWebhookEventServices,
            IStripeService stripeService,
            IPaymentServices paymentServices,
            IUserSubscriptionServices userSubscriptionServices,
            ISubscriptionPlanServices subscriptionPlanServices,
            UserManager<ApplicationUser> userManager,
            ILogger<StripeWebhookEventController> logger)
        {
            _stripeWebhookEventServices = stripeWebhookEventServices;
            _stripeService = stripeService;
            _paymentServices = paymentServices;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
            _userManager = userManager;
            _logger = logger;
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

        /// <summary>
        /// Endpoint pubblico per ricevere webhook da Stripe
        /// </summary>
        [HttpPost("stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

                if (string.IsNullOrEmpty(stripeSignature))
                {
                    _logger.LogWarning("Webhook ricevuto senza firma Stripe");
                    return BadRequest("Firma mancante");
                }

                Event stripeEvent;
                try
                {
                    stripeEvent = _stripeService.ConstructWebhookEvent(json, stripeSignature);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore nella verifica della firma del webhook");
                    return BadRequest("Firma non valida");
                }

                _logger.LogInformation($"Webhook ricevuto: {stripeEvent.Type}");

                // Verifica se l'evento è già stato processato
                var isProcessed = await _stripeWebhookEventServices.IsEventProcessedAsync(stripeEvent.Id);
                if (isProcessed)
                {
                    _logger.LogInformation($"Evento {stripeEvent.Id} già processato");
                    return Ok();
                }

                // Salva l'evento nel database
                var webhookEventModel = new StripeWebhookEventCreateModel
                {
                    EventId = stripeEvent.Id,
                    Type = stripeEvent.Type,
                    Data = json,
                    Processed = false,
                    ReceivedAt = DateTime.UtcNow
                };

                await _stripeWebhookEventServices.CreateAsync(webhookEventModel);

                // Processa l'evento in base al tipo
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    case "customer.subscription.created":
                        await HandleSubscriptionCreated(stripeEvent);
                        break;

                    case "customer.subscription.updated":
                        await HandleSubscriptionUpdated(stripeEvent);
                        break;

                    case "customer.subscription.deleted":
                        await HandleSubscriptionDeleted(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation($"Evento non gestito: {stripeEvent.Type}");
                        break;
                }

                // Marca l'evento come processato
                var webhookEvent = await _stripeWebhookEventServices.GetByEventIdAsync(stripeEvent.Id);
                if (webhookEvent != null)
                {
                    await _stripeWebhookEventServices.MarkAsProcessedAsync(webhookEvent.Id);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing del webhook Stripe");
                return StatusCode(500, "Errore interno del server");
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation($"Payment Intent succeeded: {paymentIntent.Id}");

            try
            {
                // Estrai metadati
                var email = paymentIntent.ReceiptEmail ?? paymentIntent.Metadata.GetValueOrDefault("email", "");
                var plan = paymentIntent.Metadata.GetValueOrDefault("plan", "");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning($"Email mancante per PaymentIntent {paymentIntent.Id}");
                    return;
                }

                // Cerca l'utente per email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"Utente non trovato per email {email}. Il pagamento verrà associato quando l'utente si registrerà.");
                    // Il pagamento verrà creato/associato durante la registrazione
                    return;
                }

                // Verifica se il pagamento esiste già
                var existingPayment = await _paymentServices.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
                
                if (existingPayment == null)
                {
                    // Crea un nuovo pagamento
                    var paymentModel = new PaymentCreateModel
                    {
                        UserId = user.Id,
                        Amount = paymentIntent.Amount / 100m, // Converti da centesimi
                        Currency = paymentIntent.Currency.ToUpper(),
                        PaymentMethod = "stripe",
                        Status = "completed",
                        StripePaymentIntentId = paymentIntent.Id,
                        TransactionId = paymentIntent.Id,
                        PaymentDate = DateTime.UtcNow,
                        Notes = $"Piano: {plan}"
                    };

                    var payment = await _paymentServices.CreateAsync(paymentModel);

                    // Cerca il piano di abbonamento
                    var plans = await _subscriptionPlanServices.GetAllAsync();
                    var subscriptionPlan = plans.FirstOrDefault(p => 
                        p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                    if (subscriptionPlan != null)
                    {
                        // Leggi metadata per verificare se è un upgrade con proration
                        var isUpgradeFromMetadata = paymentIntent.Metadata.GetValueOrDefault("isUpgrade", "false").ToLower() == "true";
                        var creditAmountStr = paymentIntent.Metadata.GetValueOrDefault("creditAmount", "0");
                        var originalAmountStr = paymentIntent.Metadata.GetValueOrDefault("originalAmount", "0");
                        var finalAmountStr = paymentIntent.Metadata.GetValueOrDefault("finalAmount", "0");
                        var currentPlanName = paymentIntent.Metadata.GetValueOrDefault("currentPlanName", "");

                        // Gestione abbonamento: upgrade, rinnovo o nuovo
                        var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id);
                        
                        if (activeSubscription != null)
                        {
                            var today = DateTime.UtcNow;
                            var isExpired = !activeSubscription.EndDate.HasValue || activeSubscription.EndDate.Value <= today;

                            // Stesso piano = RINNOVO
                            if (activeSubscription.SubscriptionPlanId == subscriptionPlan.Id)
                            {
                                if (!isExpired)
                                {
                                    // ABBONAMENTO NON SCADUTO: estendi la data di scadenza
                                    var newEndDate = activeSubscription.EndDate.Value.AddMonths(subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12);

                                    var updateModel = new UserSubscriptionUpdateModel
                                    {
                                        Id = activeSubscription.Id,
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = activeSubscription.StartDate, // Mantieni data originale
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = activeSubscription.AutoRenew,
                                        LastPaymentId = payment.Id,
                                        StripeSubscriptionId = activeSubscription.StripeSubscriptionId,
                                        StripeCustomerId = activeSubscription.StripeCustomerId
                                    };

                                    await _userSubscriptionServices.UpdateAsync(updateModel);
                                    _logger.LogInformation($"Abbonamento rinnovato (esteso) per {user.Email}. Piano: {subscriptionPlan.Name}, Nuova scadenza: {updateModel.EndDate}");
                                }
                                else
                                {
                                    // ABBONAMENTO SCADUTO: nuova sottoscrizione con data di decorrenza = oggi
                                    var newStartDate = today;
                                    var newEndDate = today.AddMonths(subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12);

                                    var updateModel = new UserSubscriptionUpdateModel
                                    {
                                        Id = activeSubscription.Id,
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = newStartDate, // NUOVA data di decorrenza
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = activeSubscription.AutoRenew,
                                        LastPaymentId = payment.Id,
                                        StripeSubscriptionId = activeSubscription.StripeSubscriptionId,
                                        StripeCustomerId = activeSubscription.StripeCustomerId
                                    };

                                    await _userSubscriptionServices.UpdateAsync(updateModel);
                                    _logger.LogInformation($"Abbonamento rinnovato (nuova sottoscrizione) per {user.Email}. Piano: {subscriptionPlan.Name}, Data decorrenza: {updateModel.StartDate}, Scadenza: {updateModel.EndDate}");
                                }
                            }
                            else
                            {
                                // Piano diverso = UPGRADE/DOWNGRADE
                                var oldPlanPrice = activeSubscription.SubscriptionPlan?.Price ?? 0;
                                var newPlanPrice = subscriptionPlan.Price;
                                bool isUpgrade = newPlanPrice > oldPlanPrice;
                                bool isDowngrade = oldPlanPrice > newPlanPrice;

                                // Se è un upgrade con proration (dai metadata), cancella vecchio e crea nuovo con periodo standard
                                if (isUpgradeFromMetadata && isUpgrade)
                                {
                                    // Cancella completamente il vecchio abbonamento
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    // Crea nuovo abbonamento con periodo standard (30 giorni per mensile)
                                    // Il credito è già stato applicato nel calcolo dell'importo pagato
                                    var newMonths = subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12;
                                    var newEndDate = today.AddMonths(newMonths);

                                    var subscriptionModel = new UserSubscriptionCreateModel
                                    {
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = today, // Nuovo ciclo parte da oggi
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = false,
                                        LastPaymentId = payment.Id
                                    };

                                    await _userSubscriptionServices.CreateAsync(subscriptionModel);

                                    _logger.LogInformation(
                                        "Upgrade con proration completato per {Email}. " +
                                        "Piano precedente: {OldPlan} (€{OldPrice}), " +
                                        "Nuovo piano: {NewPlan} (€{NewPrice}), " +
                                        "Credito applicato: €{Credit}, " +
                                        "Importo originale: €{Original}, " +
                                        "Importo pagato: €{Final}, " +
                                        "Nuova scadenza: {EndDate}",
                                        user.Email, currentPlanName, oldPlanPrice,
                                        subscriptionPlan.Name, newPlanPrice,
                                        creditAmountStr, originalAmountStr, finalAmountStr, newEndDate);
                                }
                                else if (isDowngrade)
                                {
                                    // Downgrade: nessun rimborso/credito applicato
                                    // Cancella il vecchio abbonamento
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    // Crea nuovo abbonamento con periodo standard
                                    var newMonths = subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12;
                                    var newEndDate = today.AddMonths(newMonths);

                                    var subscriptionModel = new UserSubscriptionCreateModel
                                    {
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = today,
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = false,
                                        LastPaymentId = payment.Id
                                    };

                                    await _userSubscriptionServices.CreateAsync(subscriptionModel);

                                    _logger.LogInformation(
                                        "Downgrade completato per {Email}. " +
                                        "Piano precedente: {OldPlan} (€{OldPrice}), " +
                                        "Nuovo piano: {NewPlan} (€{NewPrice}), " +
                                        "Nessun rimborso applicato, Scadenza: {EndDate}",
                                        user.Email, activeSubscription.SubscriptionPlan?.Name ?? "N/A", oldPlanPrice,
                                        subscriptionPlan.Name, newPlanPrice, newEndDate);
                                }
                                else
                                {
                                    // Cambio piano generico (non upgrade/downgrade riconosciuto)
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    var newMonths = subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12;
                                    var newEndDate = today.AddMonths(newMonths);

                                    var subscriptionModel = new UserSubscriptionCreateModel
                                    {
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = today,
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = false,
                                        LastPaymentId = payment.Id
                                    };

                                    await _userSubscriptionServices.CreateAsync(subscriptionModel);

                                    _logger.LogInformation(
                                        "Cambio piano completato per {Email}. Piano: {Plan}, Scadenza: {EndDate}",
                                        user.Email, subscriptionPlan.Name, newEndDate);
                                }
                            }
                        }
                        else
                        {
                            // Nuovo abbonamento (nessun abbonamento attivo)
                            var subscriptionModel = new UserSubscriptionCreateModel
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = DateTime.UtcNow,
                                EndDate = DateTime.UtcNow.AddMonths(subscriptionPlan.BillingPeriod == "monthly" ? 1 : 12),
                                Status = "active",
                                AutoRenew = false,
                                LastPaymentId = payment.Id
                            };

                            await _userSubscriptionServices.CreateAsync(subscriptionModel);
                            _logger.LogInformation($"Nuovo abbonamento creato per {user.Email} - Piano: {subscriptionPlan.Name}");
                        }
                    }
                }
                else
                {
                    // Aggiorna il pagamento esistente
                    var updateModel = new PaymentUpdateModel
                    {
                        Id = existingPayment.Id,
                        UserId = user.Id,
                        Amount = existingPayment.Amount,
                        Currency = existingPayment.Currency,
                        PaymentMethod = existingPayment.PaymentMethod,
                        Status = "completed",
                        StripePaymentIntentId = existingPayment.StripePaymentIntentId,
                        TransactionId = existingPayment.TransactionId,
                        PaymentDate = existingPayment.PaymentDate,
                        Notes = existingPayment.Notes
                    };

                    await _paymentServices.UpdateAsync(updateModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing del payment intent succeeded {paymentIntent.Id}");
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning($"Payment Intent failed: {paymentIntent.Id}");

            try
            {
                var existingPayment = await _paymentServices.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
                
                if (existingPayment != null)
                {
                    var updateModel = new PaymentUpdateModel
                    {
                        Id = existingPayment.Id,
                        UserId = existingPayment.UserId,
                        Amount = existingPayment.Amount,
                        Currency = existingPayment.Currency,
                        PaymentMethod = existingPayment.PaymentMethod,
                        Status = "failed",
                        StripePaymentIntentId = existingPayment.StripePaymentIntentId,
                        TransactionId = existingPayment.TransactionId,
                        PaymentDate = existingPayment.PaymentDate,
                        Notes = $"{existingPayment.Notes} - Pagamento fallito: {paymentIntent.LastPaymentError?.Message}"
                    };

                    await _paymentServices.UpdateAsync(updateModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing del payment intent failed {paymentIntent.Id}");
            }
        }

        private async Task HandleSubscriptionCreated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation($"Subscription created: {subscription.Id}");
            // Logica aggiuntiva se necessaria
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation($"Subscription updated: {subscription.Id}");
            
            try
            {
                var userSubscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscription.Id);
                if (userSubscription != null)
                {
                    // Aggiorna lo stato in base allo stato Stripe
                    var newStatus = subscription.Status switch
                    {
                        "active" => "active",
                        "canceled" => "cancelled",
                        "past_due" => "expired",
                        _ => userSubscription.Status
                    };

                    if (newStatus != userSubscription.Status)
                    {
                        var updateModel = new UserSubscriptionUpdateModel
                        {
                            Id = userSubscription.Id,
                            UserId = userSubscription.UserId,
                            SubscriptionPlanId = userSubscription.SubscriptionPlanId,
                            StartDate = userSubscription.StartDate,
                            EndDate = userSubscription.EndDate,
                            Status = newStatus,
                            AutoRenew = userSubscription.AutoRenew,
                            StripeSubscriptionId = userSubscription.StripeSubscriptionId,
                            StripeCustomerId = userSubscription.StripeCustomerId
                        };

                        await _userSubscriptionServices.UpdateAsync(updateModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing della subscription updated {subscription.Id}");
            }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation($"Subscription deleted: {subscription.Id}");

            try
            {
                var userSubscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscription.Id);
                if (userSubscription != null)
                {
                    await _userSubscriptionServices.CancelSubscriptionAsync(userSubscription.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing della subscription deleted {subscription.Id}");
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
