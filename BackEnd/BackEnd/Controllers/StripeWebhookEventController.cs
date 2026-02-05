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

                    case "invoice.paid":
                        await HandleInvoicePaid(stripeEvent, json);
                        break;

                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailed(stripeEvent, json);
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

            _logger.LogInformation($"Payment Intent succeeded: {paymentIntent.Id}, Status: {paymentIntent.Status}");

            try
            {
                // Estrai metadati
                var email = paymentIntent.ReceiptEmail ?? paymentIntent.Metadata.GetValueOrDefault("email", "");
                var plan = paymentIntent.Metadata.GetValueOrDefault("plan", "");
                var isRecurringPayment = paymentIntent.Metadata.GetValueOrDefault("isRecurringPayment", "false").ToLower() == "true";
                var renewalWithRecurring = paymentIntent.Metadata.GetValueOrDefault("renewalWithRecurring", "false").ToLower() == "true";

                _logger.LogInformation($"Payment Intent {paymentIntent.Id} - Email: {email}, Plan: {plan}, IsRecurring: {isRecurringPayment}, RenewalWithRecurring: {renewalWithRecurring}");

                // NON creare mai Payment o abbonamento per importo zero: evita abbonamenti "gratis" per errore
                if (paymentIntent.Amount <= 0)
                {
                    _logger.LogWarning($"PaymentIntent {paymentIntent.Id} ignorato: importo zero (Amount: {paymentIntent.Amount}). Nessun Payment o Subscription creati.");
                    return;
                }

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
                    // Pagamento ricorrente (subscription): non creare Payment qui, lo creerà invoice.paid con SubscriptionId
                    if (isRecurringPayment)
                    {
                        _logger.LogInformation($"Payment Intent {paymentIntent.Id} proviene da una subscription. Il Payment verrà creato da invoice.paid con SubscriptionId.");
                        return;
                    }

                    // Crea un nuovo pagamento (solo one-time)
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
                    _logger.LogInformation($"Pagamento creato: {payment.Id} per Payment Intent {paymentIntent.Id}");

                    // Cerca il piano di abbonamento
                    var plans = await _subscriptionPlanServices.GetAllAsync();
                    var subscriptionPlan = plans.FirstOrDefault(p => 
                        p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                    if (subscriptionPlan != null)
                    {
                        int? subscriptionIdForPayment = null;
                        // Leggi metadata per verificare se è un upgrade con proration
                        var isUpgradeFromMetadata = paymentIntent.Metadata.GetValueOrDefault("isUpgrade", "false").ToLower() == "true";
                        var creditAmountStr = paymentIntent.Metadata.GetValueOrDefault("creditAmount", "0");
                        var originalAmountStr = paymentIntent.Metadata.GetValueOrDefault("originalAmount", "0");
                        var finalAmountStr = paymentIntent.Metadata.GetValueOrDefault("finalAmount", "0");
                        var currentPlanName = paymentIntent.Metadata.GetValueOrDefault("currentPlanName", "");

                        // Gestione abbonamento: upgrade, rinnovo o nuovo (usa AdminId per trovare la stessa subscription che usa il frontend)
                        var activeSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);
                        
                        if (activeSubscription != null)
                        {
                            var today = DateTime.UtcNow;
                            var isExpired = !activeSubscription.EndDate.HasValue || activeSubscription.EndDate.Value <= today;
                            
                            // Verifica se è un trial (piano Free o senza StripeSubscriptionId)
                            var isTrial = string.IsNullOrEmpty(activeSubscription.StripeSubscriptionId) || 
                                        activeSubscription.SubscriptionPlan?.Name?.Equals("Free", StringComparison.OrdinalIgnoreCase) == true;

                            // Stesso piano = RINNOVO
                            if (activeSubscription.SubscriptionPlanId == subscriptionPlan.Id)
                            {
                                // Se AutoRenew è già true, non permettere rinnovo manuale (il rinnovo è automatico)
                                if (activeSubscription.AutoRenew == true)
                                {
                                    _logger.LogWarning($"Tentativo di rinnovo manuale per abbonamento con AutoRenew=true. Utente: {user.Email}, Piano: {subscriptionPlan.Name}. Il rinnovo è automatico, ignorando il pagamento.");
                                    return; // Ignora il pagamento, il rinnovo avverrà automaticamente
                                }
                                
                                if (!isExpired)
                                {
                                    // ABBONAMENTO NON SCADUTO: estendi la data di scadenza
                                    var newEndDate = GetEndDateFromBillingPeriod(activeSubscription.EndDate.Value, subscriptionPlan.BillingPeriod);

                                    // Se isRecurringPayment o renewalWithRecurring è true, aggiorna AutoRenew a true
                                    var newAutoRenew = (isRecurringPayment || renewalWithRecurring) ? true : activeSubscription.AutoRenew;

                                    var updateModel = new UserSubscriptionUpdateModel
                                    {
                                        Id = activeSubscription.Id,
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = activeSubscription.StartDate, // Mantieni data originale
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = newAutoRenew,
                                        LastPaymentId = payment.Id,
                                        StripeSubscriptionId = activeSubscription.StripeSubscriptionId,
                                        StripeCustomerId = activeSubscription.StripeCustomerId
                                    };

                                    await _userSubscriptionServices.UpdateAsync(updateModel);
                                    subscriptionIdForPayment = activeSubscription.Id;
                                    _logger.LogInformation($"Abbonamento rinnovato (esteso) per {user.Email}. Piano: {subscriptionPlan.Name}, Nuova scadenza: {updateModel.EndDate}, AutoRenew: {newAutoRenew}");
                                }
                                else
                                {
                                    // ABBONAMENTO SCADUTO: nuova sottoscrizione con data di decorrenza = oggi
                                    var newStartDate = today;
                                    var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);

                                    // Se isRecurringPayment o renewalWithRecurring è true, aggiorna AutoRenew a true
                                    var newAutoRenew = (isRecurringPayment || renewalWithRecurring) ? true : activeSubscription.AutoRenew;

                                    var updateModel = new UserSubscriptionUpdateModel
                                    {
                                        Id = activeSubscription.Id,
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = newStartDate, // NUOVA data di decorrenza
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = newAutoRenew,
                                        LastPaymentId = payment.Id,
                                        StripeSubscriptionId = activeSubscription.StripeSubscriptionId,
                                        StripeCustomerId = activeSubscription.StripeCustomerId
                                    };

                                    await _userSubscriptionServices.UpdateAsync(updateModel);
                                    subscriptionIdForPayment = activeSubscription.Id;
                                    _logger.LogInformation($"Abbonamento rinnovato (nuova sottoscrizione) per {user.Email}. Piano: {subscriptionPlan.Name}, Data decorrenza: {updateModel.StartDate}, Scadenza: {updateModel.EndDate}, AutoRenew: {newAutoRenew}");
                                }
                            }
                            else
                            {
                                // Piano diverso = UPGRADE/DOWNGRADE
                                var oldPlanPrice = activeSubscription.SubscriptionPlan?.Price ?? 0;
                                var newPlanPrice = subscriptionPlan.Price;
                                bool isUpgrade = newPlanPrice > oldPlanPrice;
                                bool isDowngrade = oldPlanPrice > newPlanPrice;

                                // Verifica i requisiti se:
                                // 1. È un downgrade (anche se l'abbonamento è scaduto)
                                // 2. L'abbonamento è scaduto (indipendentemente da upgrade/downgrade)
                                // Non verifica se è un upgrade e l'abbonamento è attivo (i limiti aumentano)
                                bool needsCompatibilityCheck = isDowngrade || isExpired;
                                
                                if (needsCompatibilityCheck)
                                {
                                    try
                                    {
                                        var compatibility = await _subscriptionLimitService.CheckDowngradeCompatibilityAsync(
                                            user.Id, 
                                            subscriptionPlan.Id, 
                                            user.AdminId);
                                        
                                        if (!compatibility.CanDowngrade)
                                        {
                                            _logger.LogWarning(
                                                "Impossibile creare/aggiornare abbonamento per {Email}. " +
                                                "Piano richiesto: {PlanName}, Requisiti non rispettati. " +
                                                "Limiti superati: {ExceededCount}. " +
                                                "Dettagli: {Message}",
                                                user.Email, subscriptionPlan.Name, 
                                                compatibility.ExceededLimitsCount, compatibility.Message);
                                            
                                            // Non creiamo/aggiorniamo l'abbonamento se i requisiti non sono rispettati
                                            // Il pagamento è già stato processato, ma l'abbonamento non viene attivato
                                            // Questo è un caso edge che dovrebbe essere gestito dal frontend, ma aggiungiamo il controllo per sicurezza
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, 
                                            "Errore durante la verifica dei requisiti per {Email} e piano {PlanName}. " +
                                            "Procedo comunque con la creazione dell'abbonamento.",
                                            user.Email, subscriptionPlan.Name);
                                        // In caso di errore, procediamo comunque (non blocchiamo il pagamento)
                                    }
                                }

                                // Se è un upgrade con proration (dai metadata), cancella vecchio e crea nuovo con periodo standard
                                if (isUpgradeFromMetadata && isUpgrade)
                                {
                                    // Cancella completamente il vecchio abbonamento (solo nel database, non su Stripe se è un trial)
                                    // Se è un trial, non c'è nulla da cancellare su Stripe
                                    if (!isTrial && !string.IsNullOrEmpty(activeSubscription.StripeSubscriptionId))
                                    {
                                        try
                                        {
                                            await _stripeService.CancelSubscriptionAsync(activeSubscription.StripeSubscriptionId);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, $"Errore durante la cancellazione della subscription Stripe {activeSubscription.StripeSubscriptionId} per {user.Email}");
                                        }
                                    }
                                    
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    // Crea nuovo abbonamento con periodo standard
                                    // Il credito è già stato applicato nel calcolo dell'importo pagato
                                    DateTime newEndDate;
                                    DateTime newStartDate;
                                    if (!isExpired && activeSubscription.EndDate.HasValue)
                                    {
                                        // Upgrade: mantieni i giorni rimanenti e aggiungi il nuovo periodo
                                        newStartDate = activeSubscription.StartDate; // Mantieni la data originale
                                        newEndDate = GetEndDateFromBillingPeriod(activeSubscription.EndDate.Value, subscriptionPlan.BillingPeriod);
                                    }
                                    else
                                    {
                                        // Abbonamento scaduto: nuovo ciclo parte da oggi
                                        newStartDate = today;
                                        newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                                    }

                                    var subscriptionModel = new UserSubscriptionCreateModel
                                    {
                                        UserId = user.Id,
                                        SubscriptionPlanId = subscriptionPlan.Id,
                                        StartDate = newStartDate,
                                        EndDate = newEndDate,
                                        Status = "active",
                                        AutoRenew = false,
                                        LastPaymentId = payment.Id
                                    };

                                    var newSub = await _userSubscriptionServices.CreateAsync(subscriptionModel);
                                    subscriptionIdForPayment = newSub.Id;

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
                                    // Cancella il vecchio abbonamento (solo nel database, non su Stripe se è un trial)
                                    if (!isTrial && !string.IsNullOrEmpty(activeSubscription.StripeSubscriptionId))
                                    {
                                        try
                                        {
                                            await _stripeService.CancelSubscriptionAsync(activeSubscription.StripeSubscriptionId);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, $"Errore durante la cancellazione della subscription Stripe {activeSubscription.StripeSubscriptionId} per {user.Email}");
                                        }
                                    }
                                    
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    // Crea nuovo abbonamento con periodo standard
                                    var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);

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

                                    var newSubDowngrade = await _userSubscriptionServices.CreateAsync(subscriptionModel);
                                    subscriptionIdForPayment = newSubDowngrade.Id;

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
                                    // Cancella il vecchio abbonamento (solo nel database, non su Stripe se è un trial)
                                    if (!isTrial && !string.IsNullOrEmpty(activeSubscription.StripeSubscriptionId))
                                    {
                                        try
                                        {
                                            await _stripeService.CancelSubscriptionAsync(activeSubscription.StripeSubscriptionId);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, $"Errore durante la cancellazione della subscription Stripe {activeSubscription.StripeSubscriptionId} per {user.Email}");
                                        }
                                    }
                                    
                                    await _userSubscriptionServices.CancelSubscriptionAsync(activeSubscription.Id);

                                    var newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);

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

                                    var newSubChange = await _userSubscriptionServices.CreateAsync(subscriptionModel);
                                    subscriptionIdForPayment = newSubChange.Id;

                                    _logger.LogInformation(
                                        "Cambio piano completato per {Email}. Piano: {Plan}, Scadenza: {EndDate}",
                                        user.Email, subscriptionPlan.Name, newEndDate);
                                }
                            }
                        }
                        else
                        {
                            // Nuovo abbonamento (nessun abbonamento attivo)
                            // Verifica sempre i requisiti per un nuovo abbonamento
                            try
                            {
                                var compatibility = await _subscriptionLimitService.CheckDowngradeCompatibilityAsync(
                                    user.Id, 
                                    subscriptionPlan.Id, 
                                    user.AdminId);
                                
                                if (!compatibility.CanDowngrade)
                                {
                                    _logger.LogWarning(
                                        "Impossibile creare nuovo abbonamento per {Email}. " +
                                        "Piano richiesto: {PlanName}, Requisiti non rispettati. " +
                                        "Limiti superati: {ExceededCount}. " +
                                        "Dettagli: {Message}",
                                        user.Email, subscriptionPlan.Name, 
                                        compatibility.ExceededLimitsCount, compatibility.Message);
                                    
                                    // Non creiamo l'abbonamento se i requisiti non sono rispettati
                                    // Il pagamento è già stato processato, ma l'abbonamento non viene attivato
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, 
                                    "Errore durante la verifica dei requisiti per nuovo abbonamento {Email} e piano {PlanName}. " +
                                    "Procedo comunque con la creazione dell'abbonamento.",
                                    user.Email, subscriptionPlan.Name);
                                // In caso di errore, procediamo comunque (non blocchiamo il pagamento)
                            }
                            
                            var newEndDateNew = GetEndDateFromBillingPeriod(DateTime.UtcNow, subscriptionPlan.BillingPeriod);
                            var subscriptionModel = new UserSubscriptionCreateModel
                            {
                                UserId = user.Id,
                                SubscriptionPlanId = subscriptionPlan.Id,
                                StartDate = DateTime.UtcNow,
                                EndDate = newEndDateNew,
                                Status = "active",
                                AutoRenew = false,
                                LastPaymentId = payment.Id
                            };

                            var newSubNew = await _userSubscriptionServices.CreateAsync(subscriptionModel);
                            subscriptionIdForPayment = newSubNew.Id;
                            _logger.LogInformation($"Nuovo abbonamento creato per {user.Email} - Piano: {subscriptionPlan.Name}");
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
                            _logger.LogInformation($"Payment {payment.Id} aggiornato con SubscriptionId {subscriptionIdForPayment}");
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

            _logger.LogInformation($"Subscription created: {subscription.Id}, Stripe status: {subscription.Status}");

            try
            {
                // Status DB: solo "active" dà accesso; incomplete/pending non dà accesso finché non arriva invoice.paid
                var dbStatus = MapStripeSubscriptionStatusToDb(subscription.Status);

                // Estrai metadata
                var email = subscription.Metadata.GetValueOrDefault("email", "");
                var plan = subscription.Metadata.GetValueOrDefault("plan", "");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning($"Email mancante per Subscription {subscription.Id}");
                    return;
                }

                // Cerca l'utente
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"Utente non trovato per email {email} nella subscription {subscription.Id}");
                    return;
                }

                // Cerca il piano
                var plans = await _subscriptionPlanServices.GetAllAsync();
                var subscriptionPlan = plans.FirstOrDefault(p => 
                    p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));

                if (subscriptionPlan == null)
                {
                    _logger.LogWarning($"Piano '{plan}' non trovato per subscription {subscription.Id}");
                    return;
                }

                // Verifica se esiste già un abbonamento attivo (usa AdminId per coerenza con il resto dell'app)
                var existingSubscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);
                
                if (existingSubscription == null)
                {
                    // Crea nuovo abbonamento con status mappato da Stripe (non hardcoded "active")
                    var endDateCreated = GetEndDateFromBillingPeriod(DateTime.UtcNow, subscriptionPlan.BillingPeriod);
                    var subscriptionModel = new UserSubscriptionCreateModel
                    {
                        UserId = user.Id,
                        SubscriptionPlanId = subscriptionPlan.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = endDateCreated,
                        Status = dbStatus,
                        AutoRenew = true,
                        StripeSubscriptionId = subscription.Id,
                        StripeCustomerId = subscription.CustomerId
                    };

                    await _userSubscriptionServices.CreateAsync(subscriptionModel);
                    _logger.LogInformation($"Abbonamento creato per subscription Stripe {subscription.Id} - Utente: {email}, Piano: {plan}, Status: {dbStatus}");
                }
                else
                {
                    // Se esiste già un abbonamento con una Stripe Subscription diversa, 
                    // significa che è un upgrade/rinnovo con pagamento ricorrente
                    // In questo caso, elimina il vecchio abbonamento e crea uno nuovo
                    if (!string.IsNullOrEmpty(existingSubscription.StripeSubscriptionId) && 
                        existingSubscription.StripeSubscriptionId != subscription.Id)
                    {
                        var today = DateTime.UtcNow;
                        var isExpired = !existingSubscription.EndDate.HasValue || existingSubscription.EndDate.Value <= today;
                        
                        // Verifica se è un upgrade confrontando i prezzi dei piani
                        var oldPlanPrice = existingSubscription.SubscriptionPlan?.Price ?? 0;
                        var newPlanPrice = subscriptionPlan.Price;
                        bool isUpgrade = newPlanPrice > oldPlanPrice;
                        bool isSamePlan = existingSubscription.SubscriptionPlanId == subscriptionPlan.Id;
                        
                        _logger.LogInformation(
                            "Upgrade/rinnovo con pagamento ricorrente rilevato per {Email}. " +
                            "Vecchia Subscription Stripe: {OldSubscriptionId}, Nuova Subscription Stripe: {NewSubscriptionId}. " +
                            "Piano vecchio: {OldPlan} (€{OldPrice}), Piano nuovo: {NewPlan} (€{NewPrice}), " +
                            "IsUpgrade: {IsUpgrade}, IsSamePlan: {IsSamePlan}, IsExpired: {IsExpired}. " +
                            "Eliminando vecchio abbonamento e creando nuovo.",
                            email, existingSubscription.StripeSubscriptionId, subscription.Id,
                            existingSubscription.SubscriptionPlan?.Name ?? "N/A", oldPlanPrice,
                            plan, newPlanPrice, isUpgrade, isSamePlan, isExpired);

                        // Cancella la vecchia subscription su Stripe
                        try
                        {
                            await _stripeService.CancelSubscriptionAsync(existingSubscription.StripeSubscriptionId);
                            _logger.LogInformation($"Vecchia subscription Stripe {existingSubscription.StripeSubscriptionId} cancellata su Stripe");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Errore durante la cancellazione della vecchia subscription Stripe {existingSubscription.StripeSubscriptionId}. Procedo comunque con l'eliminazione dal database.");
                        }

                        // Cancella il vecchio abbonamento dal database
                        await _userSubscriptionServices.CancelSubscriptionAsync(existingSubscription.Id);
                        _logger.LogInformation($"Vecchio abbonamento {existingSubscription.Id} eliminato dal database");

                        // Crea nuovo abbonamento con la nuova Stripe Subscription
                        // USA LA STESSA LOGICA DELL'UPGRADE NORMALE per preservare i giorni rimanenti
                        DateTime newStartDate;
                        DateTime newEndDate;
                        
                        if (!isExpired && existingSubscription.EndDate.HasValue)
                        {
                            // UPGRADE/RINNOVO: mantieni i giorni rimanenti e aggiungi il nuovo periodo
                            // Esempio: se mancano 3 giorni (scade il 29 gennaio, oggi è 26 gennaio)
                            // Il nuovo abbonamento parte da oggi (26 gennaio) e dura fino al 29 gennaio + 1 mese = 29 febbraio
                            // Questo preserva i 3 giorni pagati e aggiunge il nuovo periodo
                            newStartDate = existingSubscription.StartDate; // Mantieni la data originale
                            newEndDate = GetEndDateFromBillingPeriod(existingSubscription.EndDate.Value, subscriptionPlan.BillingPeriod);
                            
                            _logger.LogInformation(
                                "Preservando giorni rimanenti per {Email}. " +
                                "Vecchia EndDate: {OldEndDate}, Nuova EndDate: {NewEndDate}, " +
                                "Giorni rimanenti preservati: {DaysRemaining}",
                                email, existingSubscription.EndDate.Value, newEndDate,
                                (existingSubscription.EndDate.Value - today).TotalDays);
                        }
                        else
                        {
                            // Abbonamento scaduto o senza EndDate: nuovo ciclo parte da oggi
                            newStartDate = today;
                            newEndDate = GetEndDateFromBillingPeriod(today, subscriptionPlan.BillingPeriod);
                            
                            _logger.LogInformation(
                                "Nuovo ciclo per abbonamento scaduto per {Email}. " +
                                "StartDate: {StartDate}, EndDate: {EndDate}",
                                email, newStartDate, newEndDate);
                        }

                        var newSubscriptionModel = new UserSubscriptionCreateModel
                        {
                            UserId = user.Id,
                            SubscriptionPlanId = subscriptionPlan.Id,
                            StartDate = newStartDate,
                            EndDate = newEndDate,
                            Status = dbStatus,
                            AutoRenew = true,
                            StripeSubscriptionId = subscription.Id,
                            StripeCustomerId = subscription.CustomerId
                        };

                        await _userSubscriptionServices.CreateAsync(newSubscriptionModel);
                        _logger.LogInformation(
                            "Nuovo abbonamento creato per subscription Stripe {SubscriptionId} - Utente: {Email}, Piano: {Plan}, " +
                            "StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}",
                            subscription.Id, email, plan, newStartDate, newEndDate, dbStatus);
                    }
                    else
                    {
                        // Aggiorna abbonamento esistente con StripeSubscriptionId (caso in cui non aveva ancora una subscription)
                        var updateModel = new UserSubscriptionUpdateModel
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
                        };

                        await _userSubscriptionServices.UpdateAsync(updateModel);
                        _logger.LogInformation($"Abbonamento aggiornato con StripeSubscriptionId {subscription.Id} - Utente: {email}, Status: {dbStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing della subscription created {subscription.Id}");
            }
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
                    // Aggiorna lo stato in base allo stato Stripe (stessa mappatura di HandleSubscriptionCreated)
                    var newStatus = MapStripeSubscriptionStatusToDb(subscription.Status);

                    // Non passare mai a "active" da subscription.updated se non c'è ancora almeno un Payment:
                    // invoice.paid può arrivare dopo subscription.updated; solo invoice.paid crea il Payment.
                    // Così eviti "abbonamento attivo senza Payment" quando il primo pagamento non è andato a buon fine.
                    if (newStatus == "active" && !userSubscription.LastPaymentId.HasValue)
                    {
                        _logger.LogInformation($"Subscription updated {subscription.Id}: Stripe status active ma nessun Payment ancora (LastPaymentId null). Mantengo status {userSubscription.Status}.");
                        newStatus = userSubscription.Status; // mantieni pending (o altro) finché non arriva invoice.paid
                    }

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

            _logger.LogInformation($"Invoice paid: {invoice.Id}");

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice, rawEventJson);

            try
            {
                // Se l'invoice ha una subscription, gestisci il pagamento ricorrente
                if (!string.IsNullOrEmpty(subscriptionId))
                {
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
                                _logger.LogWarning($"Invoice paid {invoice.Id}: metadata email/plan mancanti su subscription {subscriptionId}");
                            }
                            else
                            {
                                var user = await _userManager.FindByEmailAsync(email);
                                if (user == null)
                                {
                                    _logger.LogWarning($"Invoice paid {invoice.Id}: utente non trovato per email {email}");
                                }
                                else
                                {
                                    var plans = await _subscriptionPlanServices.GetAllAsync();
                                    var subscriptionPlan = plans.FirstOrDefault(p => p.Name.Equals(plan, StringComparison.OrdinalIgnoreCase));
                                    if (subscriptionPlan == null)
                                    {
                                        _logger.LogWarning($"Invoice paid {invoice.Id}: piano '{plan}' non trovato");
                                    }
                                    else
                                    {
                                        var endDateInvoice = GetEndDateFromBillingPeriod(DateTime.UtcNow, subscriptionPlan.BillingPeriod);
                                        var subscriptionModel = new UserSubscriptionCreateModel
                                        {
                                            UserId = user.Id,
                                            SubscriptionPlanId = subscriptionPlan.Id,
                                            StartDate = DateTime.UtcNow,
                                            EndDate = endDateInvoice,
                                            Status = "active",
                                            AutoRenew = true,
                                        StripeSubscriptionId = subscriptionId,
                                        StripeCustomerId = stripeSubscription.CustomerId
                                        };
                                        await _userSubscriptionServices.CreateAsync(subscriptionModel);
                                        _logger.LogInformation($"UserSubscription creata da invoice.paid per subscription {subscriptionId} - Utente: {email}");
                                        subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscriptionId);
                                    }
                                }
                            }
                        }
                        catch (Exception exFallback)
                        {
                            _logger.LogError(exFallback, $"Fallback creazione subscription da invoice.paid {invoice.Id} fallito");
                        }
                    }

                    if (subscription != null)
                    {
                        // Non creare Payment a importo zero né estendere l'abbonamento per fatture a zero (es. trial)
                        var amountPaid = invoice.AmountPaid;
                        if (amountPaid <= 0)
                        {
                            _logger.LogInformation($"Invoice paid {invoice.Id} ignorata per subscription {subscriptionId}: importo zero (AmountPaid: {amountPaid}). Nessun Payment creato né estensione periodo.");
                            return;
                        }

                        // Idempotenza: evita duplicati se invoice.paid arriva più volte
                        var existingPayment = await _paymentServices.GetByTransactionIdAsync(invoice.Id);
                        PaymentSelectModel? payment;
                        if (existingPayment != null)
                        {
                            payment = existingPayment;
                            _logger.LogInformation($"Payment già esistente per invoice {invoice.Id}, aggiorno solo UserSubscription");
                        }
                        else
                        {
                            var paymentModel = new PaymentCreateModel
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
                            };
                            payment = await _paymentServices.CreateAsync(paymentModel);
                        }

                        // Aggiorna la data di scadenza: al RINNOVO estendi dalla scadenza corrente; al PRIMO PAGAMENTO
                        // non aggiungere un secondo periodo (la subscription è già stata creata con EndDate corretta in customer.subscription.created).
                        var today = DateTime.UtcNow;
                        bool isRenewal = invoice.BillingReason == "subscription_cycle";
                        DateTime newEndDate;
                        if (isRenewal && subscription.EndDate.HasValue && subscription.EndDate.Value >= today)
                        {
                            newEndDate = GetEndDateFromBillingPeriod(subscription.EndDate.Value, subscription.SubscriptionPlan?.BillingPeriod);
                        }
                        else
                        {
                            // Primo pagamento: mantieni la EndDate già impostata alla creazione, oppure calcola da oggi se mancante
                            newEndDate = subscription.EndDate.HasValue && subscription.EndDate.Value >= today
                                ? subscription.EndDate.Value
                                : GetEndDateFromBillingPeriod(today, subscription.SubscriptionPlan?.BillingPeriod);
                        }

                        var updateModel = new UserSubscriptionUpdateModel
                        {
                            Id = subscription.Id,
                            UserId = subscription.UserId,
                            SubscriptionPlanId = subscription.SubscriptionPlanId,
                            StartDate = subscription.StartDate,
                            EndDate = newEndDate,
                            Status = "active",
                            AutoRenew = subscription.AutoRenew,
                            LastPaymentId = payment?.Id,
                            StripeSubscriptionId = subscription.StripeSubscriptionId,
                            StripeCustomerId = subscription.StripeCustomerId
                        };

                        await _userSubscriptionServices.UpdateAsync(updateModel);
                        _logger.LogInformation($"Pagamento ricorrente processato per subscription {subscriptionId} - Invoice: {invoice.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing dell'invoice paid {invoice.Id}");
            }
        }

        private async Task HandleInvoicePaymentFailed(Event stripeEvent, string? rawEventJson = null)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogWarning($"Invoice payment failed: {invoice.Id}");

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice, rawEventJson);

            try
            {
                if (!string.IsNullOrEmpty(subscriptionId))
                {
                    var subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(subscriptionId);
                    if (subscription != null)
                    {
                        // Crea record di pagamento fallito
                        var paymentModel = new PaymentCreateModel
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
                        };

                        await _paymentServices.CreateAsync(paymentModel);

                        // Se è l'ultimo tentativo, marca l'abbonamento come scaduto
                        if (invoice.AttemptCount >= 3)
                        {
                            var updateModel = new UserSubscriptionUpdateModel
                            {
                                Id = subscription.Id,
                                UserId = subscription.UserId,
                                SubscriptionPlanId = subscription.SubscriptionPlanId,
                                StartDate = subscription.StartDate,
                                EndDate = subscription.EndDate,
                                Status = "expired",
                                AutoRenew = false,
                                StripeSubscriptionId = subscription.StripeSubscriptionId,
                                StripeCustomerId = subscription.StripeCustomerId
                            };

                            await _userSubscriptionServices.UpdateAsync(updateModel);
                            _logger.LogWarning($"Abbonamento {subscriptionId} scaduto dopo {invoice.AttemptCount} tentativi falliti");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il processing dell'invoice payment failed {invoice.Id}");
            }
        }

        /// <summary>
        /// Calcola il numero di mesi in base al BillingPeriod
        /// </summary>
        private static int GetMonthsFromBillingPeriod(string? billingPeriod)
        {
            if (string.IsNullOrEmpty(billingPeriod))
                return 1; // Default a 1 mese

            return billingPeriod.ToLower() switch
            {
                "monthly" => 1,
                "quarterly" => 3,
                "semiannual" => 6,
                "annual" => 12,
                "yearly" => 12, // Manteniamo compatibilità con il vecchio valore
                _ => 1 // Default a 1 mese se non riconosciuto
            };
        }

        /// <summary>
        /// Calcola la data di scadenza in base al BillingPeriod.
        /// </summary>
        private static DateTime GetEndDateFromBillingPeriod(DateTime fromDate, string? billingPeriod)
        {
            var months = GetMonthsFromBillingPeriod(billingPeriod);
            return fromDate.AddMonths(months);
        }
    }

    public class ProcessWebhookEventModel
    {
        public string EventId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Data { get; set; } = null!;
    }
}
