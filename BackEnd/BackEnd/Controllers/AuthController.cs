using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AutoMapper;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IBusinessServices;
using BackEnd.Models.AuthModels;
using BackEnd.Models.MailModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.UserModel;
using BackEnd.Models.UserSubscriptionModels;
using BackEnd.Models.PaymentModels;
using BackEnd.Models.SubscriptionPlanModels;
using BackEnd.Services.BusinessServices;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Issuing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("/api/[controller]/")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUserSubscriptionServices _userSubscriptionServices;
        private readonly ISubscriptionPlanServices _subscriptionPlanServices;
        private readonly IPaymentServices _paymentServices;
        private string SecretForKey;
        private readonly IKeyVaultSecretProvider _secretProvider;
        public AuthController(
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMailService mailService, IConfiguration configuration, IMapper mapper, IUserSubscriptionServices userSubscriptionServices, ISubscriptionPlanServices subscriptionPlanServices, IPaymentServices paymentServices,
            IKeyVaultSecretProvider secretProvider)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _mailService = mailService;
            _configuration = configuration;
            _mapper = mapper;
            _userSubscriptionServices = userSubscriptionServices;
            _subscriptionPlanServices = subscriptionPlanServices;
            _paymentServices = paymentServices;
            //secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("KeyVault:Url")), new DefaultAzureCredential());
            //KeyVaultSecret secret = secretClient.GetSecret(_configuration.GetValue<string>("KeyVault:Secrets:AuthKey"));
            SecretForKey = _configuration.GetValue<string>("Authentication:DevelopmentKey");//secret.Value;
            _secretProvider = secretProvider;

            var secretName = _configuration.GetValue<string>("KeyVault:Secrets:AuthKey");
            SecretForKey = _secretProvider.GetSecret(secretName, "Authentication:DevelopmentKey")
                ?? _configuration.GetValue<string>("Authentication:DevelopmentKey")
                ?? throw new InvalidOperationException("Key di firma JWT non configurata.");
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var userEmailExists = await userManager.FindByEmailAsync(model.Email);
                var userNameExists = await userManager.FindByNameAsync(model.UserName);
                if (userEmailExists != null || userNameExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Utente già registrato!" });


                model.UserName = model.UserName.Replace(" ", "_");

                ApplicationUser user = _mapper.Map<ApplicationUser>(model);
                user.SecurityStamp = Guid.NewGuid().ToString();

                var result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded && result.Errors.First().Code == "PasswordRequiresUpper")
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "La password deve contenere almeno una lettera maiuscola!" });
                else if (!result.Succeeded && result.Errors.First().Code == "PasswordRequiresNonAlphanumeric")
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "La password deve contenere almeno un carattere speciale!" });
                else if (!result.Succeeded && result.Errors.First().Code == "PasswordTooShort")
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "La password deve contenere almeno 8 caratteri!" });
                else if (!result.Succeeded && result.Errors.First().Code == "PasswordRequiresDigit")
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "La password deve contenere almeno un numero!" });

                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "Si è verificato un errore! Controllare i dati inseriti e provare nuovamente" });

                if (model.Role == "Admin")
                {
                    ApplicationUser newUser = await userManager.FindByEmailAsync(user.Email);
                    newUser.AdminId = newUser.Id;
                    await userManager.UpdateAsync(newUser);

                    // ===== CREAZIONE TRIAL DI 10 GIORNI PER ADMIN =====
                    try
                    {
                        // Recupera il piano Free (trial di benvenuto)
                        var allPlans = await _subscriptionPlanServices.GetActivePlansAsync();
                        var freePlan = allPlans.FirstOrDefault(p => p.Name.Equals("Free", StringComparison.OrdinalIgnoreCase));

                        if (freePlan == null)
                        {
                            // Se il piano Free non esiste, logga un errore ma non blocca la registrazione
                            Console.WriteLine($"⚠️ ATTENZIONE: Piano Free non trovato. Impossibile creare trial per Admin {newUser.Email}");
                        }
                        else
                        {
                            // Calcola le date: inizio oggi, fine tra 10 giorni
                            var startDate = DateTime.UtcNow;
                            var endDate = startDate.AddDays(10);

                            // Crea il Payment con importo 0€
                            var paymentModel = new PaymentCreateModel
                            {
                                UserId = newUser.Id,
                                Amount = 0,
                                Currency = "EUR",
                                PaymentDate = startDate,
                                PaymentMethod = "trial",
                                Status = "completed",
                                Notes = "Periodo di prova gratuito di 10 giorni"
                            };

                            var payment = await _paymentServices.CreateAsync(paymentModel);

                            // Crea la UserSubscription con piano Free
                            var subscriptionModel = new UserSubscriptionCreateModel
                            {
                                UserId = newUser.Id,
                                SubscriptionPlanId = freePlan.Id, // Piano Free
                                StartDate = startDate,
                                EndDate = endDate,
                                AutoRenew = false, // Il trial non si rinnova automaticamente
                                Status = "active",
                                LastPaymentId = payment.Id,
                                StripeSubscriptionId = null, // Nessuna subscription Stripe per il trial
                                StripeCustomerId = null
                            };

                            var subscription = await _userSubscriptionServices.CreateAsync(subscriptionModel);

                            Console.WriteLine($"✅ Trial di 10 giorni creato per Admin {newUser.Email}. Piano: Free, Scadenza: {endDate:yyyy-MM-dd}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Logga l'errore ma non blocca la registrazione
                        Console.WriteLine($"❌ Errore durante la creazione del trial per Admin {user.Email}: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                    // ===== FINE CREAZIONE TRIAL =====
                }

                var roleResult = await userManager.AddToRoleAsync(user, model.Role);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // ===== MODALITÀ TEST: Link di conferma email =====
                // Link che porta alla pagina di conferma email
                var confirmationLink = $"http://localhost:5173/email-confirmation/{user.Email}/{token}";
                Console.WriteLine("========================================");
                Console.WriteLine("LINK DI CONFERMA REGISTRAZIONE (TEST):");
                Console.WriteLine(confirmationLink);
                Console.WriteLine("========================================");

                // ===== MODALITÀ PRODUZIONE: Invio email di conferma =====
                // Decommentare le righe seguenti per l'invio effettivo delle email in produzione
                // var confirmationLink = $"https://www.amministrazionethinkhome.it/#/email-confirmation/{user.Email}/{token}";
                // MailRequest mailRequest = new MailRequest()
                // {
                //     ToEmail = user.Email,
                //     Subject = "Conferma la tua email - KuramaHome",
                //     Body = $"<h2>Benvenuto in KuramaHome!</h2><p>Per attivare le tue credenziali e completare la registrazione, <a href='{confirmationLink}'>clicca qui</a></p><p>Se il link non funziona, copia e incolla questo URL nel tuo browser:</p><p>{confirmationLink}</p><p>Il link scadrà tra 24 ore.</p>"
                // };
                // await _mailService.SendEmailAsync(mailRequest);

                return Ok(new AuthResponseModel { Status = "Success", Message = "User created successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore durante la registrazione: " + ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                model.Email = model.Email.Replace(" ", "_");
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Credenziali non valide" });
                }

                if (!user.EmailConfirmed)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Account temporaneamente bloccato." });
                }

                if (await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);

                    var subscriptionExpiry = subscription?.EndDate ?? DateTime.MinValue;
                    var userRoles = await userManager.GetRolesAsync(user);
                    string role = userRoles.Contains("Admin") ? "Admin" : userRoles.Contains("Agency") ? "Agency" : userRoles.Contains("Agent") ? "Agent" : userRoles.FirstOrDefault() ?? "";
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("subscription_expiry", subscriptionExpiry.ToString("o")),
                        new Claim("plan", subscription?.SubscriptionPlan?.Name ?? "none")
                    };

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretForKey));
                    var token = new JwtSecurityToken(
                        issuer: _configuration["Authentication:Issuer"],
                        audience: _configuration["Authentication:Audience"],
                        expires: DateTime.Now.AddMinutes(30),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    // Genera un refresh token con durata maggiore (30 giorni)
                    var refreshClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email!),
                        new Claim("type", "refresh_token")
                    };

                    var refreshToken = new JwtSecurityToken(
                        issuer: _configuration["Authentication:Issuer"],
                        audience: _configuration["Authentication:Audience"],
                        expires: DateTime.Now.AddDays(30),
                        claims: refreshClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    LoginResponse result = new LoginResponse()
                    {
                        Id = user.Id,
                        AgencyId = user.AdminId ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!,
                        Password = "",
                        Role = role,
                        Color = user.Color,
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken)
                    };

                    return Ok(result);
                }
                return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponseModel() { Status = "Error", Message = "Credenziali non valide" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore durante il login: " + ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(VerifyToken))]
        public async Task<IActionResult> VerifyToken(TokenVerificationModel api_token)
        {
            try
            {
                var u = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(SecretForKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = _configuration["Authentication:Issuer"],
                    ValidAudience = _configuration["Authentication:Audience"],

                    ClockSkew = TimeSpan.Zero // Imposta lo skew dell'orologio a zero per evitare eventuali problemi di sincronizzazione
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(api_token.api_token, validationParameters, out validatedToken);

                if (principal.Identity.IsAuthenticated)
                {
                    // Estrae l'email dal claim corretto (ClaimTypes.Email)
                    var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    if (emailClaim == null)
                    {
                        return BadRequest("Email claim non trovata nel token");
                    }

                    string email = emailClaim.Value;
                    var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    string userId = userIdClaim?.Value ?? "";

                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        return NotFound("Utente non trovato");
                    }

                    // Recupera l'abbonamento aggiornato dell'utente (con ereditarietà)
                    var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(user.Id, user.AdminId);
                    var subscriptionExpiry = subscription?.EndDate ?? DateTime.MinValue;

                    var userRoles = await userManager.GetRolesAsync(user);
                    string role = userRoles.Contains("Admin") ? "Admin"
                        : userRoles.Contains("Agency") ? "Agency"
                        : userRoles.Contains("Agent") ? "Agent"
                        : userRoles.FirstOrDefault() ?? "";

                    var result = new
                    {
                        Id = user.Id,
                        AgencyId = user.AdminId ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = role,
                        Color = user.Color,
                        SubscriptionExpiry = subscriptionExpiry,
                        Token = api_token.api_token  // Restituisce la stringa del token, non l'oggetto
                    };

                    return Ok(result);
                }

                return Unauthorized();
            }
            catch (SecurityTokenException ex)
            {
                return StatusCode(401);
            }
        }

        [HttpPost]
        [Route(nameof(ConfirmCredentials))]
        public async Task<IActionResult> ConfirmCredentials(CredentialConfirmationModel credentials)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Token))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new AuthResponseModel()
                    {
                        Status = "Error",
                        Message = "Email e token sono richiesti per la conferma delle credenziali."
                    });
                }

                var user = await userManager.FindByEmailAsync(credentials.Email);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new AuthResponseModel()
                    {
                        Status = "Error",
                        Message = "Utente non trovato. Verifica che l'email sia corretta."
                    });
                }

                // Check if email is already confirmed
                if (user.EmailConfirmed)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new AuthResponseModel()
                    {
                        Status = "Error",
                        Message = "L'email è già stata confermata. Puoi procedere con l'accesso."
                    });
                }

                var result = await userManager.ConfirmEmailAsync(user, credentials.Token);

                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    var updateResult = await userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        return Ok(new AuthResponseModel()
                        {
                            Status = "Success",
                            Message = "Email confermata con successo! Ora puoi accedere al tuo account."
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel()
                        {
                            Status = "Error",
                            Message = "Errore durante l'aggiornamento dell'utente. Riprova più tardi."
                        });
                    }
                }

                // Handle specific token errors
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new AuthResponseModel()
                    {
                        Status = "Error",
                        Message = "Token non valido. Richiedi un nuovo link di conferma."
                    });
                }

                return StatusCode(StatusCodes.Status400BadRequest, new AuthResponseModel()
                {
                    Status = "Error",
                    Message = "Token non valido o scaduto. Richiedi un nuovo link di conferma."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel()
                {
                    Status = "Error",
                    Message = "Si è verificato un errore durante la conferma delle credenziali. Riprova più tardi."
                });
            }
        }

        [HttpGet]
        [Route(nameof(KeyGen))]
        public async Task<IActionResult> KeyGen()
        {
            try
            {
                byte[] keyBytes = new byte[32];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(keyBytes);
                }

                string secretKey = Convert.ToBase64String(keyBytes);
                return Ok(secretKey);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore durante la generazione della chiave: " + ex.Message });
            }
        }

        [HttpPost]
        [Route(nameof(CreateRole))]
        public async Task<IActionResult> CreateRole(string role)
        {
            try
            {
                IdentityRole identityRole = new IdentityRole()
                {
                    Name = role
                };
                var roleAdded = await roleManager.CreateAsync(identityRole);

                return Ok(roleAdded);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpGet]
        [Route(nameof(GetUser))]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);

                var roles = await userManager.GetRolesAsync(user);

                var res = string.Join(", ", roles);
                UserSelectModel result = _mapper.Map<UserSelectModel>(user);
                if (roles.Count() > 1)
                {
                    if (roles.Contains("Admin"))
                    {
                        result.Role = "Admin";
                    }
                    else if (roles.Contains("Agency"))
                    {
                        result.Role = "Agenzia";
                    }
                }
                else
                {
                    result.Role = roles.First() == "Agency" ? result.Role = "Agenzia"
                        : roles.First() == "Agent" ? result.Role = "Agente"
                        : result.Role;
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpPost]
        [Route(nameof(SendResetLink))]
        public async Task<IActionResult> SendResetLink(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest();
                }

                ApplicationUser user = await userManager.FindByEmailAsync(email);

                string token = await userManager.GeneratePasswordResetTokenAsync(user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route(nameof(ResetPassword))]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                var resetPassResult = await userManager.ResetPasswordAsync(user, model.Token.Replace("_", "/").Replace("&", "+"), model.Password);

                if (resetPassResult.Succeeded)
                    return new OkObjectResult("Password Modificata");
                else
                {
                    string error = string.Empty;
                    if (resetPassResult.Errors.First().Code == "PasswordRequiresUpper")
                        throw new NullReferenceException("La password deve contenere almeno una lettera maiuscola!");
                    else if (resetPassResult.Errors.First().Code == "PasswordRequiresNonAlphanumeric")
                        throw new NullReferenceException("La password deve contenere almeno un carattere speciale!");
                    else if (resetPassResult.Errors.First().Code == "PasswordTooShort")
                        throw new NullReferenceException("La password deve contenere almeno 8 caratteri!");
                    else if (resetPassResult.Errors.First().Code == "PasswordRequiresDigit")
                        throw new NullReferenceException("La password deve contenere almeno un numero!");
                    else
                        throw new Exception("Si è verificato un errore!");
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });

                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore!" });
            }
        }

        [HttpPost]
        [Route(nameof(RefreshToken))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token))
                {
                    return BadRequest(new AuthResponseModel { Status = "Error", Message = "Token non fornito" });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(SecretForKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = _configuration["Authentication:Issuer"],
                    ValidAudience = _configuration["Authentication:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(model.Token, validationParameters, out validatedToken);

                if (!principal.Identity.IsAuthenticated)
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Token non valido" });
                }

                // Verifica che sia un refresh token
                var tokenTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == "type" && c.Value == "refresh_token");
                if (tokenTypeClaim == null)
                {
                    return Unauthorized(new AuthResponseModel { Status = "Error", Message = "Token non è un refresh token valido" });
                }

                // Estrae l'email dal claim corretto
                var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (emailClaim == null || userIdClaim == null)
                {
                    return BadRequest(new AuthResponseModel { Status = "Error", Message = "Claims non validi nel token" });
                }

                string email = emailClaim.Value;
                string userId = userIdClaim.Value;

                // Verifica che l'utente esista ancora
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new AuthResponseModel { Status = "Error", Message = "Utente non trovato" });
                }

                // Recupera l'abbonamento aggiornato dell'utente (con ereditarietà)
                var subscription = await _userSubscriptionServices.GetActiveUserSubscriptionAsync(userId, user.AdminId);
                var subscriptionExpiry = subscription?.EndDate ?? DateTime.MinValue;

                // Recupera i ruoli dell'utente
                var userRoles = await userManager.GetRolesAsync(user);
                string role = userRoles.Contains("Admin") ? "Admin"
                    : userRoles.Contains("Agency") ? "Agenzia"
                    : userRoles.Contains("Agent") ? "Agente"
                    : userRoles.FirstOrDefault() ?? "";

                // Crea i nuovi claims con i dati aggiornati
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("subscription_expiry", subscriptionExpiry.ToString("o")),
                    new Claim("plan", subscription?.SubscriptionPlan?.Name ?? "none")
                };

                // Genera il nuovo access token
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretForKey));
                var newToken = new JwtSecurityToken(
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    expires: DateTime.Now.AddMinutes(30),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // Genera un nuovo refresh token
                var refreshClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("type", "refresh_token")
                };

                var newRefreshToken = new JwtSecurityToken(
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    expires: DateTime.Now.AddDays(30),
                    claims: refreshClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // Prepara la risposta con i token aggiornati
                var result = new LoginResponse()
                {
                    Id = user.Id,
                    AgencyId = user.AdminId ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    Password = "",
                    Role = role,
                    Color = user.Color,
                    Token = new JwtSecurityTokenHandler().WriteToken(newToken),
                    RefreshToken = new JwtSecurityTokenHandler().WriteToken(newRefreshToken)
                };

                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new AuthResponseModel { Status = "Error", Message = $"Token non valido: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AuthResponseModel { Status = "Error", Message = $"Errore durante l'aggiornamento del token: {ex.Message}" });
            }
        }

        //[HttpGet]
        //[Route(nameof(ForgotPassword))]
        //public async Task<IActionResult> ForgotPassword(string email)
        //{
        //    var user = await userManager.FindByEmailAsync(email);
        //    if (user == null) return NotFound("NOT_FOUND");

        //    var token = await userManager.GeneratePasswordResetTokenAsync(user);

        //    MailRequest mailRequest = new MailRequest()
        //    {
        //        ToEmail = email,
        //        Subject = $"Richiesta reset password",
        //        Body = $"Ci è pervenuta una richiesta di reset password! <br><br> Se non sei stato tu, ignora questo messaggio! " +
        //         $"<br><br> Se vuoi resettare la tua password <a href='https://wepp.art/resetpassword/" + token.Replace("/", "_").Replace("+", "&") + "/" + email + "'>clicca qui!</a>" +
        //         $"<br /> <br>" +
        //         $"<br /><br /> Team Weppart"
        //    };
        //    await _mailService.SendEmailAsync(mailRequest);

        //    return Ok();

        //}

        //[HttpPost]
        //[Route(nameof(UpdatePassword))]
        //public async Task<IActionResult> UpdatePassword([FromForm] ResetPasswordModel model)
        //{
        //    try
        //    {
        //        var user = await userManager.FindByEmailAsync(model.email);
        //        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        //        var resetPassResult = await userManager.ResetPasswordAsync(user, token, model.password);

        //        if (resetPassResult.Succeeded)
        //            return new OkObjectResult("Password Modificata");
        //        else
        //        {
        //            string error = string.Empty;
        //            if (resetPassResult.Errors.First().Code == "PasswordRequiresUpper")
        //                throw new NullReferenceException("La password deve contenere almeno una lettera maiuscola!");
        //            else if (resetPassResult.Errors.First().Code == "PasswordRequiresNonAlphanumeric")
        //                throw new NullReferenceException("La password deve contenere almeno un carattere speciale!");
        //            else if (resetPassResult.Errors.First().Code == "PasswordTooShort")
        //                throw new NullReferenceException("La password deve contenere almeno 8 caratteri!");
        //            else if (resetPassResult.Errors.First().Code == "PasswordRequiresDigit")
        //                throw new NullReferenceException("La password deve contenere almeno un numero!");
        //            else
        //                throw new Exception("Si è verificato un errore!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is NullReferenceException)
        //            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = ex.Message });

        //        else
        //            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Si è verificato un errore!" });
        //    }

        //}
    }
}
