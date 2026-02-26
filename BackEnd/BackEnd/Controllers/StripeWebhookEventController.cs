using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.StripeWebhookEventModels;
using BackEnd.Models.PaymentModels;
using BackEnd.Models.UserSubscriptionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BackEnd.Entities;
using Npgsql;
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
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StripeWebhookEventController> _logger;

        public StripeWebhookEventController(
            IStripeWebhookEventServices stripeWebhookEventServices,
            IStripeService stripeService,
            IPaymentServices paymentServices,
            IUserSubscriptionServices userSubscriptionServices,
            ISubscriptionPlanServices subscriptionPlanServices,
            ISubscriptionLimitService subscriptionLimitService,
            UserManager<ApplicationUser> userManager,
            ILogger<StripeWebhookEventController> logger)
        {
            _stripeWebhookEventServices = stripeWebhookEventServices;
            _stripeService = stripeService;
            _paymentServices = paymentServices;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
            _subscriptionLimitService = subscriptionLimitService;
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

                _logger.LogInformation("Webhook ricevuto: {EventType}", stripeEvent.Type);

                var existingEvent = await _stripeWebhookEventServices.GetByEventIdAsync(stripeEvent.Id);
                if (existingEvent != null && existingEvent.Processed)
                {
                    _logger.LogInformation("Evento {EventId} già processato", stripeEvent.Id);
                    return Ok();
                }
                if (existingEvent == null)
                {
                    await _stripeWebhookEventServices.CreateAsync(new StripeWebhookEventCreateModel
                    {
                        EventId = stripeEvent.Id,
                        Type = stripeEvent.Type,
                        Data = json,
                        Processed = false,
                        ReceivedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogInformation("Evento {EventId} già presente, skip Create e continua processing", stripeEvent.Id);
                }

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
                    case "invoice.paid":
                        await HandleInvoicePaid(stripeEvent, json);
                        break;
                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailed(stripeEvent, json);
                        break;
                    default:
                        _logger.LogInformation("Evento non gestito: {EventType}", stripeEvent.Type);
                        break;
                }

                var webhookEvent = await _stripeWebhookEventServices.GetByEventIdAsync(stripeEvent.Id);
                if (webhookEvent != null)
                    await _stripeWebhookEventServices.MarkAsProcessedAsync(webhookEvent.Id);

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

            _logger.LogInformation("Payment Intent succeeded: {PaymentIntentId}, Status: {Status}",
                paymentIntent.Id, paymentIntent.Status);

            try
            {
                var email = paymentIntent.ReceiptEmail ?? paymentIntent.Metadata.GetValueOrDefault("email", "");
                var plan = paymentIntent.Metadata.GetValueOrDefault("plan", "");
                var isRecurringPayment = paymentIntent.Metadata.GetValueOrDefault("isRecurringPayment", "false").ToLower() == "true";
                var renewalWithRecurring = paymentIntent.Metadata.GetValueOrDefault("renewalWithRecurring", "false").ToLower() == "true";

                _logger.LogInformation(
                    "Payment Intent {PaymentIntentId} - Email: {Email}, Plan: {Plan}, IsRecurring: {IsRecurring}, RenewalWithRecurring: {RenewalWithRecurring}",
                    paymentIntent.Id, email, plan, isRecurringPayment, renewalWithRecurring);

                if (paymentIntent.Amount <= 0)
                {
                    _logger.LogWarning(
                        "PaymentIntent {PaymentIntentId} ignorato: importo zero (Amount: {Amount}). Nessun Payment o Subscription creati.",
                        paymentIntent.Id, paymentIntent.Amount);
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email mancante per PaymentIntent {PaymentIntentId}", paymentIntent.Id);
                    return;
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning(
                        "Utente non trovato per email {Email}. Il pagamento verrà associato quando l'utente si registrerà.",
                        email);
                    return;
                }

                var existingPayment = await _paymentServices.GetByStripePaymentIntentIdAsync(paymentIntent.Id);

                if (existingPayment == null)
                {
                    // Pagamenti da subscription: il Payment viene creato da invoice.paid con SubscriptionId corretto
                    if (isRecurringPayment)
                    {
                        _logger.LogInformation(
                            "Payment Intent {PaymentIntentId} proviene da una subscription. Il Payment verrà creato da invoice.paid.",
                            paymentIntent.Id);
                        return;
                    }

                    var payment = await _paymentServices.CreateAsync(new PaymentCreateModel
                    {
                        UserId = user.Id,
                        Amount = paymentIntent.Amount / 100m,
                        Currency = paymentIntent.Currency.ToUpper(),
                        PaymentMethod = "stripe",
                        Status = "completed",
                        StripePaymentIntentId = paymentIntent.Id,
                        TransactionId = paymentIntent.Id,
                        PaymentDate = DateTime.UtcNow,
                        Notes = $"Piano: {plan}"
                    });

                    _logger.LogInformation("Pagamento creato: {PaymentId} per Payment Intent {PaymentIntentId}",
                        payment.Id, paymentIntent.Id);

                    var plans = await _subscriptionPlanServices.GetAllAsync();
                    var subscriptionPlan = plans.FirstOrDefault(p =>
                        p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                    if (subscriptionPlan != null)
                    {
                        int? subscriptionIdForPayment = null;

                        var isUpgradeFromMetadata = paymentIntent.Metadata.GetValueOrDefault("isUpgrade", "false").ToLower() == "true";
                        var creditAmountStr = paymentIntent.Metadata.GetValueOrDefault("creditAmount", "0");
                        var originalAmountStr = paymentIntent.Metadata.GetValueOrDefault("originalAmount", "0");
                        var finalAmountStr = paymentIntent.Metadata.GetValueOrDefault("finalAmount", "0");
                        var currentPlanName = paymentIntent.Metadata.GetValueOrDefault("currentPlanName", "");

                        var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);

                        if (activeSubscription != null)
                        {
                            var today = DateTime.UtcNow;
                            var isExpired = !activeSubscription.EndDate.HasValue || activeSubscription.EndDate.Value <= today;
                            var isTrial = string.IsNullOrEmpty(activeSubscription.StripeSubscriptionId) ||
                                          activeSubscription.SubscriptionPlan?.Name?.Equals("Free", StringComparison.OrdinalIgnoreCase) == true;

                            if (activeSubscription.SubscriptionPlanId == subscriptionPlan.Id)
                            {
                                // RINNOVO: stesso piano
                                if (activeSubscription.AutoRenew)
                                {
                                    _logger.LogWarning(
                                        "Tentativo di rinnovo manuale per abbonamento con AutoRenew=true. Utente: {Email}, Piano: {Plan}. Il rinnovo è automatico, ignorando il pagamento.",
                                        user.Email, subscriptionPlan.Name);
                                    return;
                                }

                                // I due rami (scaduto / non scaduto) differiscono solo per StartDate e fonte della EndDate
                                var newAutoRenew = (isRecurringPayment || renewalWithRecurring) ? true : activeSubscription.AutoRenew;
                                var newStartDate = isExpired ? today : activeSubscription.StartDate;
                                var endDateBase = isExpired ? today : activeSubscription.EndDate!.Value;
                                var newEndDate = GetEndDateFromBillingPeriod(endDateBase, subscriptionPlan.BillingPeriod);

                                await _userSubscriptionServices.UpdateAsync(new UserSubscriptionUpdateModel
                                {
                                    Id = activeSubscription.Id,
                                    UserId = user.Id,
                                    SubscriptionPlanId = subscriptionPlan.Id,
                                    StartDate = newStartDate,
                                    EndDate = newEndDate,
                                    Status = "active",
                                    AutoRenew = newAutoRenew,
                                    LastPaymentId = payment.Id,
                                    StripeSubscriptionId = activeSubscription.StripeSubscriptionId,
                                    StripeCustomerId = activeSubscription.StripeCustomerId
                                });

                                subscriptionIdForPayment = activeSubscription.Id;
                                _logger.LogInformation(
                                    "Abbonamento rinnovato per {Email}. Piano: {Plan}, StartDate: {Start}, EndDate: {End}, AutoRenew: {AutoRenew}",
                                    user.Email, subscriptionPlan.Name, newStartDate, newEndDate, newAutoRenew);
                            }
                            else
                            {
                                // Piano diverso: UPGRADE / DOWNGRADE / CAMBIO GENERICO
                                var oldPlanPrice = activeSubscription.SubscriptionPlan?.Price ?? 0;
                                var newPlanPrice = subscriptionPlan.Price;
                                bool isUpgrade = newPlanPrice > oldPlanPrice;
                                bool isDowngrade = oldPlanPrice > newPlanPrice;
                                bool needsCompatibilityCheck = isDowngrade || isExpired;

                                if (needsCompatibilityCheck)
                                {
                                    try
                                    {
                                        var compatibility = await _subscriptionLimitService.CheckDowngradeCompatibilityAsync(
                                            user.Id, subscriptionPlan.Id, user.AdminId);

                                        if (!compatibility.CanDowngrade)
                                        {
                                            _logger.LogWarning(
                                                "Impossibile creare/aggiornare abbonamento per {Email}. Piano richiesto: {PlanName}, Requisiti non rispettati. Limiti superati: {ExceededCount}. Dettagli: {Message}",
                                                user.Email, subscriptionPlan.Name, compatibility.ExceededLimitsCount, compatibility.Message);
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex,
                                            "Errore durante la verifica dei requisiti per {Email} e piano {PlanName}. Procedo comunque.",
                                            user.Email, subscriptionPlan.Name);
                                    }
                                }

                                // Tutti i cambi di piano hanno la stessa struttura: cancella vecchio, crea nuovo da oggi
                                await CancelOldSubscriptionAsync(activeSubscription, isTrial);

                                var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                                var newSub = await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                                {
                                    UserId = user.Id,
                                    SubscriptionPlanId = subscriptionPlan.Id,
                                    StartDate = today,
                                    EndDate = newEndDate,
                                    Status = "active",
                                    AutoRenew = false,
                                    LastPaymentId = payment.Id
                                });

                                subscriptionIdForPayment = newSub.Id;

                                if (isUpgradeFromMetadata && isUpgrade)
                                    _logger.LogInformation(
                                        "Upgrade con proration completato per {Email}. Piano precedente: {OldPlan} (€{OldPrice}), Nuovo piano: {NewPlan} (€{NewPrice}), Credito applicato: €{Credit}, Importo originale: €{Original}, Importo pagato: €{Final}, Nuova scadenza: {EndDate}",
                                        user.Email, currentPlanName, oldPlanPrice, subscriptionPlan.Name, newPlanPrice,
                                        creditAmountStr, originalAmountStr, finalAmountStr, newEndDate);
                                else if (isDowngrade)
                                    _logger.LogInformation(
                                        "Downgrade completato per {Email}. Piano precedente: {OldPlan} (€{OldPrice}), Nuovo piano: {NewPlan} (€{NewPrice}), Scadenza: {EndDate}",
                                        user.Email, activeSubscription.SubscriptionPlan?.Name ?? "N/A", oldPlanPrice,
                                        subscriptionPlan.Name, newPlanPrice, newEndDate);
                                else
                                    _logger.LogInformation(
                                        "Cambio piano completato per {Email}. Piano: {Plan}, Scadenza: {EndDate}",
                                        user.Email, subscriptionPlan.Name, newEndDate);
                            }
                        }
                        else
                        {
                            // Nessun abbonamento attivo: nuovo abbonamento
                            try
                            {
                                var compatibility = await _subscriptionLimitService.CheckDowngradeCompatibilityAsync(
                                    user.Id, subscriptionPlan.Id, user.AdminId);

                                if (!compatibility.CanDowngrade)
                                {
                                    _logger.LogWarning(
                                        "Impossibile creare nuovo abbonamento per {Email}. Piano richiesto: {PlanName}, Requisiti non rispettati. Limiti superati: {ExceededCount}. Dettagli: {Message}",
                                        user.Email, subscriptionPlan.Name, compatibility.ExceededLimitsCount, compatibility.Message);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Errore durante la verifica dei requisiti per nuovo abbonamento {Email} e piano {PlanName}. Procedo comunque.",
                                    user.Email, subscriptionPlan.Name);
                            }

                            var today = DateTime.UtcNow;
                            var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                            var newSub = await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = today,
                                EndDate = newEndDate,
                                Status = "active",
                                AutoRenew = false,
                                LastPaymentId = payment.Id
                            });

                            subscriptionIdForPayment = newSub.Id;
                            _logger.LogInformation("Nuovo abbonamento creato per {Email} - Piano: {Plan}",
                                user.Email, subscriptionPlan.Name);
                        }

                        if (subscriptionIdForPayment.HasValue)
                        {
                            await _paymentServices.UpdateAsync(new PaymentUpdateModel
                            {
                                Id = payment.Id,
                                UserId = payment.UserId,
                                SubscriptionId = subscriptionIdForPayment,
                                Amount = payment.Amount,
                                Currency = payment.Currency,
                                PaymentDate = payment.PaymentDate,
                                PaymentMethod = payment.PaymentMethod,
                                TransactionId = payment.TransactionId,
                                Status = payment.Status,
                                Notes = payment.Notes,
                                StripePaymentIntentId = payment.StripePaymentIntentId
                            });
                            _logger.LogInformation("Payment {PaymentId} aggiornato con SubscriptionId {SubscriptionId}",
                                payment.Id, subscriptionIdForPayment);
                        }
                    }
                }
                else
                {
                    await _paymentServices.UpdateAsync(new PaymentUpdateModel
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
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing del payment intent succeeded {PaymentIntentId}",
                    paymentIntent.Id);
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning("Payment Intent failed: {PaymentIntentId}", paymentIntent.Id);

            try
            {
                var existingPayment = await _paymentServices.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
                if (existingPayment != null)
                {
                    await _paymentServices.UpdateAsync(new PaymentUpdateModel
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
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing del payment intent failed {PaymentIntentId}",
                    paymentIntent.Id);
            }
        }

        /// <summary>
        /// Mappa lo status della subscription Stripe allo status interno DB.
        /// Solo "active" dà accesso; tutto il resto è non attivo (pending, expired, ecc.).
        /// </summary>
        private static string MapStripeSubscriptionStatusToDb(string? stripeStatus)
        {
            return (stripeStatus?.ToLowerInvariant()) switch
            {
                "active" => "active",
                "incomplete" => "pending",
                "incomplete_expired" => "expired",
                "trialing" => "pending",
                "past_due" => "past_due",
                "canceled" => "cancelled",
                "unpaid" => "expired",
                _ => "pending"
            };
        }

        private async Task HandleSubscriptionCreated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription created: {SubscriptionId}, Stripe status: {Status}",
                subscription.Id, subscription.Status);

            try
            {
                var dbStatus = MapStripeSubscriptionStatusToDb(subscription.Status);
                var email = subscription.Metadata.GetValueOrDefault("email", "");
                var plan = subscription.Metadata.GetValueOrDefault("plan", "");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email mancante per Subscription {SubscriptionId}", subscription.Id);
                    return;
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Utente non trovato per email {Email} nella subscription {SubscriptionId}",
                        email, subscription.Id);
                    return;
                }

                var plans = await _subscriptionPlanServices.GetAllAsync();
                var subscriptionPlan = plans.FirstOrDefault(p =>
                    p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                if (subscriptionPlan == null)
                {
                    _logger.LogWarning("Piano '{Plan}' non trovato per subscription {SubscriptionId}", plan, subscription.Id);
                    return;
                }

                var existingSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);

                if (existingSubscription == null)
                {
                    var endDate = GetEndDateFromBillingPeriod(DateTime.UtcNow, subscriptionPlan.BillingPeriod);
                    await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                    {
                        UserId = user.Id,
                        SubscriptionPlanId = subscriptionPlan.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = endDate,
                        Status = dbStatus,
                        AutoRenew = true,
                        StripeSubscriptionId = subscription.Id,
                        StripeCustomerId = subscription.CustomerId
                    });
                    _logger.LogInformation(
                        "Abbonamento creato per subscription Stripe {SubscriptionId} - Utente: {Email}, Piano: {Plan}, Status: {Status}",
                        subscription.Id, email, plan, dbStatus);
                }
                else
                {
                    // Hoist isOldPlanFree to avoid duplicate computation in both branches below
                    var isOldPlanFree = existingSubscription.SubscriptionPlan?.Name?.Equals("Free", StringComparison.OrdinalIgnoreCase) == true;

                    if (!string.IsNullOrEmpty(existingSubscription.StripeSubscriptionId) &&
                        existingSubscription.StripeSubscriptionId != subscription.Id)
                    {
                        // Upgrade/rinnovo con nuova Stripe subscription: crea il nuovo abbonamento in pending.
                        // Il vecchio viene cancellato solo dopo invoice.paid per non lasciare l'utente senza piano.
                        var today = DateTime.UtcNow;
                        var isExpired = !existingSubscription.EndDate.HasValue || existingSubscription.EndDate.Value <= today;
                        var oldPlanPrice = existingSubscription.SubscriptionPlan?.Price ?? 0;
                        var newPlanPrice = subscriptionPlan.Price;
                        bool isUpgrade = newPlanPrice > oldPlanPrice;
                        bool isSamePlan = existingSubscription.SubscriptionPlanId == subscriptionPlan.Id;

                        _logger.LogInformation(
                            "Upgrade/rinnovo con pagamento ricorrente rilevato per {Email}. " +
                            "Vecchia Subscription Stripe: {OldSubscriptionId}, Nuova Subscription Stripe: {NewSubscriptionId}. " +
                            "Piano vecchio: {OldPlan} (€{OldPrice}), Piano nuovo: {NewPlan} (€{NewPrice}), " +
                            "IsUpgrade: {IsUpgrade}, IsSamePlan: {IsSamePlan}, IsExpired: {IsExpired}. " +
                            "Creando nuovo abbonamento in pending; il vecchio verrà cancellato solo dopo invoice.paid.",
                            email, existingSubscription.StripeSubscriptionId, subscription.Id,
                            existingSubscription.SubscriptionPlan?.Name ?? "N/A", oldPlanPrice,
                            plan, newPlanPrice, isUpgrade, isSamePlan, isExpired);

                        DateTime newStartDate, newEndDate;
                        if (!isExpired && existingSubscription.EndDate.HasValue && !isOldPlanFree)
                        {
                            // Mantieni i giorni rimanenti dell'abbonamento corrente
                            newStartDate = existingSubscription.StartDate;
                            newEndDate = GetEndDateFromBillingPeriod(existingSubscription.EndDate.Value, subscriptionPlan.BillingPeriod);
                            _logger.LogInformation(
                                "Preservando giorni rimanenti per {Email}. Vecchia EndDate: {OldEndDate}, Nuova EndDate: {NewEndDate}, Giorni rimasti: {DaysRemaining}",
                                email, existingSubscription.EndDate.Value, newEndDate,
                                (existingSubscription.EndDate.Value - today).TotalDays);
                        }
                        else
                        {
                            newStartDate = today;
                            newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                            _logger.LogInformation(
                                "Nuovo ciclo per abbonamento scaduto per {Email}. StartDate: {StartDate}, EndDate: {EndDate}",
                                email, newStartDate, newEndDate);
                        }

                        await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                        {
                            UserId = user.Id,
                            SubscriptionPlanId = subscriptionPlan.Id,
                            StartDate = newStartDate,
                            EndDate = newEndDate,
                            Status = dbStatus,
                            AutoRenew = true,
                            StripeSubscriptionId = subscription.Id,
                            StripeCustomerId = subscription.CustomerId
                        });
                        _logger.LogInformation(
                            "Nuovo abbonamento creato per subscription Stripe {SubscriptionId} - Utente: {Email}, Piano: {Plan}, StartDate: {Start}, EndDate: {End}, Status: {Status}",
                            subscription.Id, email, plan, newStartDate, newEndDate, dbStatus);
                    }
                    else
                    {
                        // Evita duplicati: invoice.paid può arrivare PRIMA di customer.subscription.created
                        // e creare già la UserSubscription con status "active". In quel caso non creare un duplicato.
                        var existingByStripeId = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscription.Id);
                        if (existingByStripeId != null)
                        {
                            _logger.LogInformation(
                                "Subscription Stripe {SubscriptionId} già presente nel DB (probabilmente da invoice.paid arrivato prima). Skip creazione duplicato per {Email}.",
                                subscription.Id, email);
                            return;
                        }

                        // Se il pagamento non è ancora confermato (es. "pending"/"incomplete"), non toccare
                        // l'abbonamento esistente: creiamo solo il nuovo in pending e attendiamo invoice.paid.
                        // La cancellazione del vecchio avverrà solo dopo il primo pagamento riuscito.
                        if (dbStatus != "active")
                        {
                            var today = DateTime.UtcNow;
                            var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                            await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = today,
                                EndDate = newEndDate,
                                Status = dbStatus,
                                AutoRenew = true,
                                StripeSubscriptionId = subscription.Id,
                                StripeCustomerId = subscription.CustomerId
                            });
                            _logger.LogInformation(
                                "Nuovo abbonamento in {Status} creato per {Email} (Piano: {Plan}). " +
                                "Abbonamento precedente mantenuto attivo fino a conferma pagamento.",
                                dbStatus, email, plan);
                        }
                        else if (isOldPlanFree)
                        {
                            // Da Free (trial) a piano a pagamento con pagamento già confermato: annulla il trial e crea nuovo ciclo pieno
                            await _userSubscriptionServices.CancelSubscriptionAsync(existingSubscription.Id);
                            var today = DateTime.UtcNow;
                            var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                            await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = today,
                                EndDate = newEndDate,
                                Status = dbStatus,
                                AutoRenew = true,
                                StripeSubscriptionId = subscription.Id,
                                StripeCustomerId = subscription.CustomerId
                            });
                            _logger.LogInformation(
                                "Upgrade da Free (trial) a piano a pagamento per {Email}. Piano {Plan}, StartDate: {Start}, EndDate: {End}",
                                email, plan, today, newEndDate);
                        }
                        else
                        {
                            // Aggiorna abbonamento esistente con la nuova Stripe subscription (pagamento già confermato)
                            await _userSubscriptionServices.UpdateAsync(new UserSubscriptionUpdateModel
                            {
                                Id = existingSubscription.Id,
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = existingSubscription.StartDate,
                                EndDate = existingSubscription.EndDate,
                                Status = dbStatus,
                                AutoRenew = true,
                                StripeSubscriptionId = subscription.Id,
                                StripeCustomerId = subscription.CustomerId
                            });
                            _logger.LogInformation(
                                "Abbonamento aggiornato con StripeSubscriptionId {SubscriptionId} - Utente: {Email}, Status: {Status}",
                                subscription.Id, email, dbStatus);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing della subscription created {SubscriptionId}",
                    subscription.Id);
            }
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription updated: {SubscriptionId}", subscription.Id);

            try
            {
                var userSubscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscription.Id);
                if (userSubscription == null) return;

                var newStatus = MapStripeSubscriptionStatusToDb(subscription.Status);

                // Non passare mai a "active" se non c'è ancora almeno un Payment:
                // invoice.paid può arrivare dopo subscription.updated e solo lì viene creato il Payment.
                if (newStatus == "active" && !userSubscription.LastPaymentId.HasValue)
                {
                    _logger.LogInformation(
                        "Subscription updated {SubscriptionId}: Stripe status active ma nessun Payment ancora (LastPaymentId null). Mantengo status {Status}.",
                        subscription.Id, userSubscription.Status);
                    newStatus = userSubscription.Status;
                }

                if (newStatus != userSubscription.Status)
                {
                    await _userSubscriptionServices.UpdateAsync(BuildSubscriptionUpdateModel(
                        userSubscription, newStatus, userSubscription.EndDate, userSubscription.AutoRenew,
                        userSubscription.LastPaymentId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing della subscription updated {SubscriptionId}",
                    subscription.Id);
            }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription deleted: {SubscriptionId}", subscription.Id);

            try
            {
                var userSubscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscription.Id);
                if (userSubscription != null)
                    await _userSubscriptionServices.CancelSubscriptionAsync(userSubscription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing della subscription deleted {SubscriptionId}",
                    subscription.Id);
            }
        }

        /// <summary>
        /// Estrae lo subscription ID dall'Invoice. Con API Stripe 2025 (Basil/Clover) subscription_id è deprecato:
        /// l'ID è in data.object.parent.subscription_details.subscription. Supporta entrambe le strutture.
        /// </summary>
        private static string? GetSubscriptionIdFromInvoice(Invoice invoice, string? rawEventJson)
        {
            if (!string.IsNullOrEmpty(invoice.SubscriptionId))
                return invoice.SubscriptionId;

            if (string.IsNullOrEmpty(rawEventJson)) return null;

            try
            {
                using var doc = JsonDocument.Parse(rawEventJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("object", out var obj))
                {
                    if (obj.TryGetProperty("subscription_id", out var subId))
                        return subId.GetString();
                    if (obj.TryGetProperty("subscription", out var sub))
                        return sub.GetString();
                    if (obj.TryGetProperty("parent", out var parent) &&
                        parent.TryGetProperty("subscription_details", out var subDetails) &&
                        subDetails.TryGetProperty("subscription", out var subNew))
                        return subNew.GetString();
                }
            }
            catch { /* ignora parse errors */ }

            return null;
        }

        private async Task HandleInvoicePaid(Event stripeEvent, string? rawEventJson = null)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogInformation("Invoice paid: {InvoiceId}", invoice.Id);

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice, rawEventJson);

            try
            {
                if (string.IsNullOrEmpty(subscriptionId)) return;

                var subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscriptionId);

                // invoice.paid può arrivare prima di customer.subscription.created: crea UserSubscription da Stripe
                if (subscription == null)
                {
                    try
                    {
                        var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscriptionId);
                        var email = stripeSubscription.Metadata.GetValueOrDefault("email", "");
                        var plan = stripeSubscription.Metadata.GetValueOrDefault("plan", "");

                        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(plan))
                        {
                            _logger.LogWarning(
                                "Invoice paid {InvoiceId}: metadata email/plan mancanti su subscription {SubscriptionId}",
                                invoice.Id, subscriptionId);
                        }
                        else
                        {
                            var user = await _userManager.FindByEmailAsync(email);
                            if (user == null)
                            {
                                _logger.LogWarning(
                                    "Invoice paid {InvoiceId}: utente non trovato per email {Email}",
                                    invoice.Id, email);
                            }
                            else
                            {
                                var plans = await _subscriptionPlanServices.GetAllAsync();
                                var subscriptionPlan = plans.FirstOrDefault(p =>
                                    p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                                if (subscriptionPlan == null)
                                {
                                    _logger.LogWarning(
                                        "Invoice paid {InvoiceId}: piano '{Plan}' non trovato", invoice.Id, plan);
                                }
                                else
                                {
                                    var endDate = GetEndDateFromBillingPeriod(DateTime.UtcNow, subscriptionPlan.BillingPeriod);
                                    await _userSubscriptionServices.CreateAsync(new UserSubscriptionCreateModel
                                    {
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = DateTime.UtcNow,
                                        EndDate = endDate,
                                        Status = "active",
                                        AutoRenew = true,
                                        StripeSubscriptionId = subscriptionId,
                                        StripeCustomerId = stripeSubscription.CustomerId
                                    });
                                    _logger.LogInformation(
                                        "UserSubscription creata da invoice.paid per subscription {SubscriptionId} - Utente: {Email}",
                                        subscriptionId, email);
                                    subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscriptionId);
                                }
                            }
                        }
                    }
                    catch (Exception exFallback)
                    {
                        _logger.LogError(exFallback,
                            "Fallback creazione subscription da invoice.paid {InvoiceId} fallito", invoice.Id);
                    }
                }

                if (subscription == null) return;

                // Non creare Payment a importo zero né estendere il periodo (es. trial)
                if (invoice.AmountPaid <= 0)
                {
                    _logger.LogInformation(
                        "Invoice paid {InvoiceId} ignorata per subscription {SubscriptionId}: importo zero. Nessun Payment creato.",
                        invoice.Id, subscriptionId);
                    return;
                }

                // Idempotenza: evita duplicati se invoice.paid arriva più volte
                var existingPayment = await _paymentServices.GetByTransactionIdAsync(invoice.Id);
                PaymentSelectModel? payment;
                if (existingPayment != null)
                {
                    payment = existingPayment;
                    _logger.LogInformation("Payment già esistente per invoice {InvoiceId}, aggiorno solo UserSubscription",
                        invoice.Id);
                }
                else
                {
                    payment = await _paymentServices.CreateAsync(new PaymentCreateModel
                    {
                        UserId = subscription.UserId,
                        SubscriptionId = subscription.Id,
                        Amount = invoice.AmountPaid / 100m,
                        Currency = invoice.Currency.ToUpper(),
                        PaymentMethod = "stripe",
                        Status = "completed",
                        StripePaymentIntentId = invoice.PaymentIntentId,
                        TransactionId = invoice.Id,
                        PaymentDate = DateTime.UtcNow,
                        Notes = string.IsNullOrEmpty(invoice.BillingReason) || invoice.BillingReason == "subscription_cycle"
                            ? $"Rinnovo automatico - Invoice: {invoice.Id}"
                            : $"Primo pagamento subscription - Invoice: {invoice.Id}"
                    });
                }

                // Al RINNOVO estendi dalla scadenza corrente; al PRIMO PAGAMENTO mantieni l'EndDate già impostata
                var today = DateTime.UtcNow;
                bool isRenewal = invoice.BillingReason == "subscription_cycle";
                DateTime newEndDate;
                if (isRenewal && subscription.EndDate.HasValue && subscription.EndDate.Value >= today)
                    newEndDate = GetEndDateFromBillingPeriod(subscription.EndDate.Value, subscription.SubscriptionPlan?.BillingPeriod);
                else
                    newEndDate = subscription.EndDate.HasValue && subscription.EndDate.Value >= today
                        ? subscription.EndDate.Value
                        : GetEndDateFromBillingPeriod(today, subscription.SubscriptionPlan?.BillingPeriod);

                await _userSubscriptionServices.UpdateAsync(
                    BuildSubscriptionUpdateModel(subscription, "active", newEndDate, subscription.AutoRenew, payment?.Id));

                _logger.LogInformation(
                    "Pagamento ricorrente processato per subscription {SubscriptionId} - Invoice: {InvoiceId}",
                    subscriptionId, invoice.Id);

                // Primo pagamento: cancella ora il vecchio abbonamento rimasto attivo.
                // Include sia le vecchie subscription Stripe sia i piani Free/una-tantum (senza StripeSubscriptionId).
                // (per i ricorrenti la cancellazione avviene qui, non in customer.subscription.created)
                if (!isRenewal)
                {
                    var allUserSubs = await _userSubscriptionServices.GetUserSubscriptionsAsync(subscription.UserId);
                    var othersActive = allUserSubs
                        .Where(s => string.Equals(s.Status, "active", StringComparison.OrdinalIgnoreCase)
                                 && s.Id != subscription.Id
                                 && (string.IsNullOrEmpty(s.StripeSubscriptionId)
                                     || s.StripeSubscriptionId != subscriptionId))
                        .ToList();

                    foreach (var oldSub in othersActive)
                    {
                        if (!string.IsNullOrEmpty(oldSub.StripeSubscriptionId))
                        {
                            try
                            {
                                await _stripeService.CancelSubscriptionAsync(oldSub.StripeSubscriptionId);
                                _logger.LogInformation(
                                    "Vecchia subscription Stripe {StripeSubId} cancellata su Stripe dopo attivazione nuovo abbonamento.",
                                    oldSub.StripeSubscriptionId);
                            }
                            catch (Exception exStripe)
                            {
                                _logger.LogWarning(exStripe,
                                    "Errore cancellazione subscription Stripe {StripeSubId}. Procedo con cancellazione in DB.",
                                    oldSub.StripeSubscriptionId);
                            }
                        }

                        await _userSubscriptionServices.CancelSubscriptionAsync(oldSub.Id);
                        _logger.LogInformation(
                            "Vecchio abbonamento DB Id {SubscriptionId} (Stripe {StripeSubId}) cancellato dopo primo pagamento.",
                            oldSub.Id, oldSub.StripeSubscriptionId ?? "N/A");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing dell'invoice paid {InvoiceId}", invoice.Id);
            }
        }

        private async Task HandleInvoicePaymentFailed(Event stripeEvent, string? rawEventJson = null)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogWarning("Invoice payment failed: {InvoiceId}", invoice.Id);

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice, rawEventJson);

            try
            {
                if (string.IsNullOrEmpty(subscriptionId)) return;

                var subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscriptionId);
                if (subscription == null) return;

                await _paymentServices.CreateAsync(new PaymentCreateModel
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id,
                    Amount = invoice.AmountDue / 100m,
                    Currency = invoice.Currency.ToUpper(),
                    PaymentMethod = "stripe",
                    Status = "failed",
                    TransactionId = invoice.Id,
                    PaymentDate = DateTime.UtcNow,
                    Notes = $"Pagamento ricorrente fallito - Invoice: {invoice.Id}, Tentativi rimasti: {invoice.AttemptCount}"
                });

                if (invoice.AttemptCount >= 3)
                {
                    await _userSubscriptionServices.UpdateAsync(
                        BuildSubscriptionUpdateModel(subscription, "expired", subscription.EndDate, false,
                            subscription.LastPaymentId));
                    _logger.LogWarning(
                        "Abbonamento {SubscriptionId} scaduto dopo {AttemptCount} tentativi falliti",
                        subscriptionId, invoice.AttemptCount);
                }
                else if (subscription.AutoRenew && invoice.AttemptCount == 1)
                {
                    await _userSubscriptionServices.UpdateAsync(
                        BuildSubscriptionUpdateModel(subscription, "past_due", subscription.EndDate, true,
                            subscription.LastPaymentId));
                    _logger.LogWarning(
                        "Abbonamento {SubscriptionId} in grazia (past_due) - primo tentativo fallito, AutoRenew attivo. Stripe riproverà nei prossimi 3 giorni.",
                        subscriptionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il processing dell'invoice payment failed {InvoiceId}",
                    invoice.Id);
            }
        }

        private static int GetMonthsFromBillingPeriod(string? billingPeriod)
        {
            return billingPeriod?.ToLower() switch
            {
                "monthly" => 1,
                "quarterly" => 3,
                "semiannual" => 6,
                "annual" => 12,
                "yearly" => 12,
                _ => 1
            };
        }

        private static DateTime GetEndDateFromBillingPeriod(DateTime fromDate, string? billingPeriod)
        {
            return fromDate.AddMonths(GetMonthsFromBillingPeriod(billingPeriod));
        }

        /// <summary>
        /// Costruisce un UserSubscriptionUpdateModel preservando tutti i campi dell'abbonamento esistente
        /// e sostituendo solo quelli esplicitamente passati.
        /// </summary>
        private static UserSubscriptionUpdateModel BuildSubscriptionUpdateModel(
            UserSubscriptionSelectModel subscription,
            string status,
            DateTime? endDate,
            bool autoRenew,
            int? lastPaymentId = null)
        {
            return new UserSubscriptionUpdateModel
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                SubscriptionPlanId = subscription.SubscriptionPlanId,
                StartDate = subscription.StartDate,
                EndDate = endDate,
                Status = status,
                AutoRenew = autoRenew,
                LastPaymentId = lastPaymentId,
                StripeSubscriptionId = subscription.StripeSubscriptionId,
                StripeCustomerId = subscription.StripeCustomerId
            };
        }

        /// <summary>
        /// Cancella l'abbonamento su Stripe (se non è un trial) e nel database.
        /// </summary>
        private async Task CancelOldSubscriptionAsync(UserSubscriptionSelectModel subscription, bool isTrial)
        {
            if (!isTrial && !string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                try
                {
                    await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Errore durante la cancellazione della subscription Stripe {StripeSubId}.",
                        subscription.StripeSubscriptionId);
                }
            }

            await _userSubscriptionServices.CancelSubscriptionAsync(subscription.Id);
        }
    }

    public class ProcessWebhookEventModel
    {
        public string EventId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Data { get; set; } = null!;
    }
}
