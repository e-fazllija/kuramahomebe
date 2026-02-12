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
                _logger.LogInformation("📥 CreatePaymentIntent chiamato - Plan: {Plan}, IsRecurringPayment: {IsRecurringPayment}, Amount: {Amount}, Email: {Email}", 
                    request.Plan, 
                    request.IsRecurringPayment, 
                    request.Amount, 
                    request.Email);
                
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

                                        // Durata ciclo attuale (in giorni). Per piani mensili usare almeno 30 giorni per evitare credito gonfiato.
                                        var cycleDuration = (endDate - startDate).TotalDays;
                                        if (cycleDuration <= 0)
                                            cycleDuration = 30;
                                        var isMonthly = currentSubscription.SubscriptionPlan?.BillingPeriod?.Equals("monthly", StringComparison.OrdinalIgnoreCase) == true;
                                        if (isMonthly && cycleDuration < 30)
                                            cycleDuration = 30;

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

                // NON permettere mai pagamenti a importo zero: nessun abbonamento deve essere attivato senza pagamento
                if (amountInCents <= 0)
                {
                    _logger.LogWarning("CreatePaymentIntent rifiutato: importo <= 0 (Plan: {Plan}, Amount: {Amount}, FinalAmount: {FinalAmount}).", request.Plan, request.Amount, finalAmount);
                    return BadRequest("L'importo del pagamento deve essere maggiore di zero. Seleziona un piano con prezzo valido.");
                }

                // Crea metadata per tracciare il piano, l'email e le info di upgrade
                var metadata = new Dictionary<string, string>
                {
                    { "plan", request.Plan },
                    { "email", request.Email ?? "" },
                    { "source", "pricing_page" },
                    { "isUpgrade", isUpgrade.ToString().ToLower() },
                    { "originalAmount", originalAmount.ToString("F2") },
                    { "creditAmount", creditAmount.ToString("F2") },
                    { "finalAmount", finalAmount.ToString("F2") },
                    { "isRecurringPayment", (request.IsRecurringPayment ?? false).ToString().ToLower() }
                };

                if (!string.IsNullOrEmpty(currentPlanId))
                {
                    metadata["currentPlanId"] = currentPlanId;
                    metadata["currentPlanName"] = currentPlanName ?? "";
                }

                // Se è un pagamento ricorrente, usa Stripe Subscription
                if (request.IsRecurringPayment == true)
                {
                    _logger.LogInformation("🔄 Pagamento ricorrente richiesto per piano: {Plan}", request.Plan);

                    if (string.IsNullOrEmpty(request.Email))
                        return BadRequest("Email richiesta per il pagamento ricorrente.");

                    var userRecurring = await _userManager.FindByEmailAsync(request.Email);
                    if (userRecurring == null)
                        return BadRequest("Utente non trovato.");

                    var currentSubRecurring = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userRecurring.Id, userRecurring.AdminId);

                    // Recupera il piano per ottenere lo StripePriceId
                    var allPlans = await _subscriptionPlanServices.GetActivePlansAsync();
                    var selectedPlan = allPlans.FirstOrDefault(p => 
                        p.Name.Equals(request.Plan, StringComparison.OrdinalIgnoreCase));

                    _logger.LogInformation("🔍 Piano cercato: {Plan}, Piano trovato: {Found}, StripePriceId: {StripePriceId}", 
                        request.Plan, 
                        selectedPlan != null ? selectedPlan.Name : "NULL",
                        selectedPlan?.StripePriceId ?? "NULL");

                    if (selectedPlan == null || string.IsNullOrEmpty(selectedPlan.StripePriceId))
                    {
                        _logger.LogWarning("❌ Piano '{Plan}' non trovato o Stripe Price ID non configurato. StripePriceId: {StripePriceId}", 
                            request.Plan, 
                            selectedPlan?.StripePriceId ?? "NULL");
                        return BadRequest($"Piano '{request.Plan}' non trovato o Stripe Price ID non configurato per pagamenti ricorrenti.");
                    }

                    // Step 2-3: calcolo credito residuo (one-time → ricorrente, stesso piano o upgrade) e applicazione prima di creare subscription
                    decimal creditAmountRecurring = 0;
                    int daysRemainingForCredit = 0;
                    if (currentSubRecurring != null && currentSubRecurring.SubscriptionPlan != null && currentSubRecurring.EndDate.HasValue)
                    {
                        var todayRecurring = DateTime.UtcNow;
                        if (currentSubRecurring.EndDate.Value > todayRecurring)
                        {
                            var endDate = currentSubRecurring.EndDate.Value;
                            var startDate = currentSubRecurring.StartDate;
                            var cycleDuration = (endDate - startDate).TotalDays;
                            if (cycleDuration <= 0) cycleDuration = 30;
                            var isMonthlyRecurring = currentSubRecurring.SubscriptionPlan?.BillingPeriod?.Equals("monthly", StringComparison.OrdinalIgnoreCase) == true;
                            if (isMonthlyRecurring && cycleDuration < 30) cycleDuration = 30;
                            var daysRemaining = (endDate - todayRecurring).TotalDays;
                            if (daysRemaining < 0) daysRemaining = 0;
                            daysRemainingForCredit = (int)daysRemaining;
                            var currentPlanPrice = currentSubRecurring.SubscriptionPlan.Price;
                            var dailyRate = currentPlanPrice / (decimal)cycleDuration;
                            creditAmountRecurring = dailyRate * (decimal)daysRemaining;
                            creditAmountRecurring = Math.Round(creditAmountRecurring, 2, MidpointRounding.AwayFromZero);
                            _logger.LogInformation(
                                "Credito residuo per passaggio a ricorrente: {Email}, giorni rimasti: {Days}, credito €{Credit}",
                                request.Email, daysRemainingForCredit, creditAmountRecurring);
                        }
                    }

                    // Rinnovo con ricorrente: ha già una subscription con AutoRenew = false e stesso piano → riattiva Stripe e usa pagamento one-time
                    if (currentSubRecurring != null && !string.IsNullOrEmpty(currentSubRecurring.StripeSubscriptionId)
                        && currentSubRecurring.AutoRenew == false && currentSubRecurring.SubscriptionPlanId == selectedPlan.Id)
                    {
                        _logger.LogInformation("Rinnovo con ricorrente per {Email}: riattivo subscription {StripeSubId}, pagamento one-time per il rinnovo.", request.Email, currentSubRecurring.StripeSubscriptionId);
                        try
                        {
                            await _stripeService.SetCancelAtPeriodEndAsync(currentSubRecurring.StripeSubscriptionId, false);
                        }
                        catch (Exception exStripe)
                        {
                            _logger.LogWarning(exStripe, "Impossibile riattivare subscription Stripe {StripeSubId}. Procedo con pagamento one-time.", currentSubRecurring.StripeSubscriptionId);
                        }
                        decimal amountRecurring = selectedPlan.Price;
                        if (creditAmountRecurring > 0 && creditAmountRecurring < amountRecurring)
                            amountRecurring = Math.Max(0, amountRecurring - creditAmountRecurring);
                        long amountCentsRenewal = (long)Math.Round(amountRecurring * 100);
                        if (amountCentsRenewal <= 0)
                            amountCentsRenewal = (long)Math.Round(selectedPlan.Price * 100);
                        if (amountCentsRenewal <= 0)
                        {
                            _logger.LogWarning("CreatePaymentIntent rifiutato (rinnovo ricorrente): importo <= 0 per piano {Plan}.", request.Plan);
                            return BadRequest("L'importo del pagamento deve essere maggiore di zero.");
                        }
                        var metadataRenewal = new Dictionary<string, string>
                        {
                            { "plan", request.Plan },
                            { "email", request.Email ?? "" },
                            { "source", "pricing_page" },
                            { "renewalWithRecurring", "true" },
                            { "userSubscriptionId", currentSubRecurring.Id.ToString() },
                            { "isRecurringPayment", "false" }
                        };
                        var paymentIntentRenewal = await _stripeService.CreatePaymentIntentAsync(amountCentsRenewal, request.Currency ?? "eur", request.Email ?? "", metadataRenewal);
                        return Ok(new CreatePaymentIntentResponse { ClientSecret = paymentIntentRenewal.ClientSecret, PaymentIntentId = paymentIntentRenewal.Id });
                    }

                    var currencyRecurring = request.Currency ?? "eur";
                    // Crea o recupera il Customer Stripe
                    var customer = await _stripeService.CreateOrGetCustomerAsync(
                        request.Email ?? "",
                        null,
                        metadata
                    );

                    // Applica credito sul customer balance PRIMA di creare la subscription (Stripe lo userà sulla prima invoice)
                    if (creditAmountRecurring > 0)
                    {
                        var creditCents = (long)Math.Round(creditAmountRecurring * 100);
                        var creditDescription = daysRemainingForCredit > 0
                            ? $"Credito residuo abbonamento precedente – {daysRemainingForCredit} giorni"
                            : "Credito residuo abbonamento precedente";
                        if (creditDescription.Length > 350) creditDescription = creditDescription.Substring(0, 350);
                        await _stripeService.CreateCustomerCreditAsync(customer.Id, creditCents, currencyRecurring, creditDescription, metadata);
                        _logger.LogInformation("Credito applicato su customer {CustomerId}: €{Credit} ({CreditCents} centesimi)", customer.Id, creditAmountRecurring, creditCents);
                    }

                    // Crea la Subscription
                    var subscription = await _stripeService.CreateSubscriptionAsync(
                        customer.Id,
                        selectedPlan.StripePriceId,
                        metadata
                    );

                    // Stripe NON copia i metadata della Subscription sul PaymentIntent della prima invoice:
                    // senza questo aggiornamento, confirm-payment non riconoscerebbe isRecurringPayment e restituirebbe 400.
                    // Se la prima invoice ha importo 0 (credito copre tutto), Stripe NON crea un PaymentIntent.
                    var piId = subscription.LatestInvoice?.PaymentIntent?.Id;
                    if (string.IsNullOrEmpty(piId))
                    {
                        _logger.LogInformation(
                            "Subscription {SubscriptionId}: prima invoice senza PaymentIntent (importo 0 dopo credito). " +
                            "Stripe pagherà automaticamente la fattura con il balance; attendere webhook invoice.paid.",
                            subscription.Id);
                        return Ok(new CreatePaymentIntentResponse
                        {
                            ClientSecret = null!,
                            PaymentIntentId = null!,
                            NoPaymentRequired = true
                        });
                    }
                    await _stripeService.UpdatePaymentIntentMetadataAsync(piId, metadata);

                    // Estrai il client secret dall'invoice del primo pagamento
                    var clientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret;
                    if (string.IsNullOrEmpty(clientSecret))
                    {
                        return StatusCode(500, "Impossibile recuperare il client secret dalla subscription.");
                    }

                    return Ok(new CreatePaymentIntentResponse
                    {
                        ClientSecret = clientSecret,
                        PaymentIntentId = piId
                    });
                }

                // Pagamento one-time: usa Payment Intent (procedura esistente)
                var paymentIntent = await _stripeService.CreatePaymentIntentAsync(
                    amountInCents,
                    request.Currency ?? "eur",
                    request.Email ?? "",
                    metadata
                );

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

                // Verifica se è un pagamento ricorrente (subscription)
                var isRecurringPayment = paymentIntent.Metadata.GetValueOrDefault("isRecurringPayment", "false").ToLower() == "true";

                // Step 7: per ricorrente, Success dipende SOLO dallo stato Stripe del PaymentIntent (non dalla presenza del Payment in DB)
                if (isRecurringPayment)
                {
                    if (paymentIntent.Status == "succeeded" || paymentIntent.Status == "processing")
                    {
                        var existingPaymentRecurring = await _paymentServices.GetByStripePaymentIntentIdAsync(request.PaymentIntentId);
                        return Ok(new PaymentConfirmationResponse
                        {
                            Success = true,
                            Message = paymentIntent.Status == "succeeded" ? "Pagamento completato" : "Pagamento in elaborazione",
                            PaymentId = existingPaymentRecurring?.Id
                        });
                    }
                    if (paymentIntent.Status == "requires_action")
                    {
                        return Ok(new PaymentConfirmationResponse
                        {
                            Success = false,
                            Message = "Completa l'autenticazione (3DS) per confermare il pagamento.",
                            PaymentId = null
                        });
                    }
                    if (paymentIntent.Status == "requires_payment_method")
                    {
                        // Recovery: verifica se l'invoice è già pagata (webhook in ritardo)
                        try
                        {
                            var userIdReq = User.FindFirstValue(ClaimTypes.NameIdentifier);
                                if (!string.IsNullOrEmpty(userIdReq))
                                {
                                    var user = await _userManager.FindByIdAsync(userIdReq);
                                    var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userIdReq, user?.AdminId);
                                    if (subscription != null && !string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                                    {
                                        var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionId);
                                        var latestInvoiceId = stripeSubscription.LatestInvoiceId ?? stripeSubscription.LatestInvoice?.Id;
                                        if (!string.IsNullOrEmpty(latestInvoiceId))
                                        {
                                            var invoice = await _stripeService.GetInvoiceAsync(latestInvoiceId);
                                            if (invoice.Status == "paid" && invoice.PaymentIntentId == request.PaymentIntentId)
                                            {
                                                _logger.LogInformation("PaymentIntent {PaymentIntentId} in requires_payment_method ma invoice {InvoiceId} è pagata - conferma successo", paymentIntent.Id, latestInvoiceId);
                                                // Crea Payment in DB se manca (recupero se invoice.paid non è arrivato)
                                                var existingPaymentForInvoice = await _paymentServices.GetByTransactionIdAsync(invoice.Id);
                                                int? paymentId = existingPaymentForInvoice?.Id;
                                                if (existingPaymentForInvoice == null)
                                                {
                                                    try
                                                    {
                                                        var paymentModel = new PaymentCreateModel
                                                        {
                                                            UserId = subscription.UserId,
                                                            SubscriptionId = subscription.Id,
                                                            Amount = invoice.AmountPaid / 100m,
                                                            Currency = invoice.Currency?.ToUpper() ?? "EUR",
                                                            PaymentMethod = "stripe",
                                                            Status = "completed",
                                                            StripePaymentIntentId = invoice.PaymentIntentId,
                                                            TransactionId = invoice.Id,
                                                            PaymentDate = DateTime.UtcNow,
                                                            Notes = invoice.BillingReason == "subscription_cycle" ? $"Rinnovo - Invoice: {invoice.Id}" : $"Primo pagamento - Invoice: {invoice.Id}"
                                                        };
                                                        var payment = await _paymentServices.CreateAsync(paymentModel);
                                                        paymentId = payment?.Id;
                                                        var today = DateTime.UtcNow;
                                                        var newEndDate = GetEndDateFromBillingPeriod(today, subscription.SubscriptionPlan?.BillingPeriod);
                                                        var updateModel = new UserSubscriptionUpdateModel
                                                        {
                                                            Id = subscription.Id,
                                                            UserId = subscription.UserId,
                                                            SubscriptionPlanId = subscription.SubscriptionPlanId,
                                                            StartDate = subscription.StartDate,
                                                            EndDate = newEndDate,
                                                            Status = "active",
                                                            AutoRenew = subscription.AutoRenew,
                                                            LastPaymentId = paymentId,
                                                            StripeSubscriptionId = subscription.StripeSubscriptionId,
                                                            StripeCustomerId = subscription.StripeCustomerId
                                                        };
                                                        await _userSubscriptionServices.UpdateAsync(updateModel);
                                                        _logger.LogInformation("Payment creato da confirm-payment per subscription {SubscriptionId}, Invoice {InvoiceId}", subscription.Id, invoice.Id);
                                                    }
                                                    catch (Exception exPay)
                                                    {
                                                        _logger.LogWarning(exPay, "Creazione Payment da confirm-payment fallita per invoice {InvoiceId}", invoice.Id);
                                                    }
                                                }
                                                return Ok(new PaymentConfirmationResponse
                                                {
                                                    Success = true,
                                                    Message = "Pagamento confermato",
                                                    PaymentId = paymentId
                                                });
                                            }
                                        }
                                }
                            }

                            // Fallback: subscription non ancora in DB (es. webhook customer.subscription.created non processato) — verifica invoice dal PaymentIntent
                            var piWithInvoice = await _stripeService.GetPaymentIntentAsync(request.PaymentIntentId, new List<string> { "invoice" });
                            var invoiceIdFromPi = piWithInvoice.InvoiceId ?? (piWithInvoice.Invoice != null ? piWithInvoice.Invoice.Id : null);
                            if (!string.IsNullOrEmpty(invoiceIdFromPi))
                            {
                                var invoiceFromPi = piWithInvoice.Invoice ?? await _stripeService.GetInvoiceAsync(invoiceIdFromPi);
                                if (invoiceFromPi != null && invoiceFromPi.Status == "paid" && invoiceFromPi.PaymentIntentId == request.PaymentIntentId)
                                {
                                    _logger.LogInformation("PaymentIntent {PaymentIntentId} in requires_payment_method, invoice {InvoiceId} pagata (fallback senza subscription in DB)", paymentIntent.Id, invoiceIdFromPi);
                                    return Ok(new PaymentConfirmationResponse
                                    {
                                        Success = true,
                                        Message = "Pagamento confermato",
                                        PaymentId = null
                                    });
                                }
                            }
                        }
                        catch (Exception exInvoice)
                        {
                            _logger.LogWarning(exInvoice, "Verifica invoice per PaymentIntent {PaymentIntentId} fallita", paymentIntent.Id);
                        }
                        // Nessun recovery: carta rifiutata
                        return Ok(new PaymentConfirmationResponse
                        {
                            Success = false,
                            Message = "Carta rifiutata o pagamento non completato. Prova un altro metodo di pagamento.",
                            PaymentId = null
                        });
                    }

                    // Altri stati ricorrente (es. canceled)
                    return Ok(new PaymentConfirmationResponse
                    {
                        Success = false,
                        Message = $"Il pagamento non è stato completato. Stato: {paymentIntent.Status}",
                        PaymentId = null
                    });
                }

                // One-time: logica esistente
                var validStatuses = new[] { "succeeded", "processing" };
                if (!validStatuses.Contains(paymentIntent.Status))
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

                // Durata ciclo attuale (in giorni). Per piani mensili usare almeno 30 giorni per evitare credito gonfiato.
                var cycleDuration = (endDate - startDate).TotalDays;
                if (cycleDuration <= 0)
                    cycleDuration = 30;
                var isMonthly = currentPlan.BillingPeriod?.Equals("monthly", StringComparison.OrdinalIgnoreCase) == true;
                if (isMonthly && cycleDuration < 30)
                    cycleDuration = 30;

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
        /// Recupera i piani per la landing page: solo Basic, Pro, Premium mensili
        /// </summary>
        [HttpGet("landing-plans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanSelectModel>>> GetLandingPlans()
        {
            try
            {
                var plans = await _subscriptionPlanServices.GetLandingPlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei piani landing");
                return StatusCode(500, $"Errore durante il recupero dei piani: {ex.Message}");
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
        /// Sincronizza il pagamento mancante per una subscription ricorrente (recupero da Stripe se invoice.paid non è arrivato).
        /// </summary>
        [HttpPost("sync-subscription-payment")]
        [Authorize]
        public async Task<ActionResult<SyncSubscriptionPaymentResponse>> SyncSubscriptionPayment([FromBody] SyncSubscriptionPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.StripeSubscriptionId) && !request.UserSubscriptionId.HasValue)
                    return BadRequest("Fornire StripeSubscriptionId o UserSubscriptionId");

                UserSubscriptionSelectModel? subscription = null;
                if (request.UserSubscriptionId.HasValue)
                {
                    subscription = await _userSubscriptionServices.GetByIdAsync(request.UserSubscriptionId.Value);
                }
                else if (!string.IsNullOrEmpty(request.StripeSubscriptionId))
                {
                    subscription = await _userSubscriptionServices.GetByStripeSubscriptionIdAsync(request.StripeSubscriptionId);
                }

                if (subscription == null)
                    return NotFound("Abbonamento non trovato");

                if (string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                    return BadRequest("L'abbonamento non è collegato a Stripe (StripeSubscriptionId mancante)");

                var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionId);
                var latestInvoiceId = stripeSubscription.LatestInvoiceId ?? stripeSubscription.LatestInvoice?.Id;
                if (string.IsNullOrEmpty(latestInvoiceId))
                    return Ok(new SyncSubscriptionPaymentResponse { Success = false, Message = "Nessuna invoice sulla subscription Stripe" });

                var invoice = await _stripeService.GetInvoiceAsync(latestInvoiceId);
                if (invoice.Status != "paid")
                    return Ok(new SyncSubscriptionPaymentResponse { Success = false, Message = $"Invoice non pagata (stato: {invoice.Status})" });

                var existingPayment = await _paymentServices.GetByTransactionIdAsync(invoice.Id);
                if (existingPayment != null)
                    return Ok(new SyncSubscriptionPaymentResponse { Success = true, Message = "Payment già presente", PaymentId = existingPayment.Id });

                var paymentModel = new PaymentCreateModel
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id,
                    Amount = invoice.AmountPaid / 100m,
                    Currency = invoice.Currency?.ToUpper() ?? "EUR",
                    PaymentMethod = "stripe",
                    Status = "completed",
                    StripePaymentIntentId = invoice.PaymentIntentId,
                    TransactionId = invoice.Id,
                    PaymentDate = DateTime.UtcNow,
                    Notes = invoice.BillingReason == "subscription_cycle" ? $"Rinnovo - Invoice: {invoice.Id}" : $"Primo pagamento - Invoice: {invoice.Id}"
                };
                var payment = await _paymentServices.CreateAsync(paymentModel);

                var today = DateTime.UtcNow;
                // Primo pagamento (LastPaymentId era null): EndDate = oggi + periodo. Rinnovo: EndDate esistente + periodo.
                var isFirstPayment = subscription.LastPaymentId == null;
                var fromDate = isFirstPayment ? today : (subscription.EndDate.HasValue && subscription.EndDate.Value > today ? subscription.EndDate.Value : today);
                var newEndDate = GetEndDateFromBillingPeriod(fromDate, subscription.SubscriptionPlan?.BillingPeriod);

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

                _logger.LogInformation($"Sync subscription payment: UserSubscription {subscription.Id}, Payment {payment?.Id}, Invoice {invoice.Id}");
                return Ok(new SyncSubscriptionPaymentResponse { Success = true, Message = "Payment sincronizzato", PaymentId = payment?.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante sync subscription payment");
                return StatusCode(500, new SyncSubscriptionPaymentResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Sincronizza la subscription in pending dell'utente corrente (dopo pagamento ricorrente, se invoice.paid non è ancora arrivato).
        /// Chiamato dal frontend dopo "Pagamento completato" per ricorrenti, così l'abbonamento diventa attivo subito senza attendere il webhook.
        /// </summary>
        [HttpPost("sync-my-pending-subscription")]
        [Authorize]
        public async Task<ActionResult<SyncSubscriptionPaymentResponse>> SyncMyPendingSubscription()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var allSubscriptions = await _userSubscriptionServices.GetUserSubscriptionsAsync(userId);
                // 1) Prova prima una subscription "pending" (pagamento appena completato in app)
                var pendingWithStripe = allSubscriptions
                    .Where(s => string.Equals(s.Status, "pending", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(s.StripeSubscriptionId))
                    .OrderByDescending(s => s.CreationDate)
                    .FirstOrDefault();

                if (pendingWithStripe != null)
                {
                    var request = new SyncSubscriptionPaymentRequest { UserSubscriptionId = pendingWithStripe.Id };
                    var result = await SyncSubscriptionPayment(request);
                    return result;
                }

                // 2) Altrimenti: abbonamento "active" ma senza Payment (es. bonifico confermato su Stripe ma webhook invoice.paid non ricevuto)
                var activeWithoutPayment = allSubscriptions
                    .Where(s => string.Equals(s.Status, "active", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(s.StripeSubscriptionId)
                        && !s.LastPaymentId.HasValue)
                    .OrderByDescending(s => s.CreationDate)
                    .FirstOrDefault();

                if (activeWithoutPayment != null)
                {
                    _logger.LogInformation("Sync my pending: trovata subscription attiva senza Payment (Id: {Id}), recupero da Stripe.", activeWithoutPayment.Id);
                    var request = new SyncSubscriptionPaymentRequest { UserSubscriptionId = activeWithoutPayment.Id };
                    var result = await SyncSubscriptionPayment(request);
                    return result;
                }

                return Ok(new SyncSubscriptionPaymentResponse { Success = false, Message = "Nessuna subscription in attesa da sincronizzare" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante sync my pending subscription");
                return StatusCode(500, new SyncSubscriptionPaymentResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un SetupIntent per permettere al cliente di aggiornare il metodo di pagamento (carta) senza uscire dall'app.
        /// L'utente deve avere un abbonamento attivo con Stripe (ricorrente o ex-ricorrente).
        /// </summary>
        [HttpPost("create-setup-intent")]
        [Authorize]
        public async Task<ActionResult<CreateSetupIntentResponse>> CreateSetupIntent()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    return BadRequest("Nessun abbonamento con metodo di pagamento salvato. Aggiorna la carta al prossimo rinnovo.");
                }

                var setupIntent = await _stripeService.CreateSetupIntentAsync(subscription.StripeCustomerId, new Dictionary<string, string>
                {
                    { "userId", userId }
                });
                return Ok(new CreateSetupIntentResponse { ClientSecret = setupIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del SetupIntent");
                return StatusCode(500, "Impossibile preparare l'aggiornamento del metodo di pagamento. Riprova più tardi.");
            }
        }

        /// <summary>
        /// Imposta il metodo di pagamento (carta) appena salvato come predefinito per il customer e per la subscription.
        /// Chiamato dal frontend dopo che l'utente ha confermato il SetupIntent con la nuova carta.
        /// </summary>
        [HttpPost("set-default-payment-method")]
        [Authorize]
        public async Task<ActionResult> SetDefaultPaymentMethod([FromBody] SetDefaultPaymentMethodRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.PaymentMethodId))
                    return BadRequest("PaymentMethodId richiesto.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized();

                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    return BadRequest("Nessun abbonamento attivo trovato.");
                }

                await _stripeService.SetDefaultPaymentMethodForCustomerAsync(subscription.StripeCustomerId, request.PaymentMethodId);
                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    await _stripeService.SetDefaultPaymentMethodForSubscriptionAsync(subscription.StripeSubscriptionId, request.PaymentMethodId);
                }
                _logger.LogInformation("Metodo di pagamento predefinito aggiornato per utente {UserId}", userId);
                return Ok(new { Message = "Metodo di pagamento aggiornato correttamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del metodo di pagamento");
                return StatusCode(500, "Impossibile aggiornare il metodo di pagamento. Riprova più tardi.");
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

        /// <summary>
        /// Calcola la data di scadenza in base al BillingPeriod.
        /// </summary>
        private static DateTime GetEndDateFromBillingPeriod(DateTime fromDate, string? billingPeriod)
        {
            var months = billingPeriod?.ToLower() == "yearly" ? 12 : (billingPeriod?.ToLower() == "monthly" ? 1 : 1);
            return fromDate.AddMonths(months);
        }
    }

    // DTOs
    public class CreatePaymentIntentRequest
    {
        public string Plan { get; set; } = null!;
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? Email { get; set; }
        public bool? IsRecurringPayment { get; set; } // true = Subscription, false/null = Payment Intent
    }

    public class CreatePaymentIntentResponse
    {
        public string? ClientSecret { get; set; }
        public string? PaymentIntentId { get; set; }
        /// <summary>True quando la prima invoice della subscription ha importo 0 (credito applicato) e non serve pagare.</summary>
        public bool NoPaymentRequired { get; set; }
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

    public class SyncSubscriptionPaymentRequest
    {
        public string? StripeSubscriptionId { get; set; }
        public int? UserSubscriptionId { get; set; }
    }

    public class SyncSubscriptionPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? PaymentId { get; set; }
    }

    public class CreateSetupIntentResponse
    {
        public string ClientSecret { get; set; } = null!;
    }

    public class SetDefaultPaymentMethodRequest
    {
        public string PaymentMethodId { get; set; } = null!;
    }
}

