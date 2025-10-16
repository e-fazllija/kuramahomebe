using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BackEnd.Entities;
using BackEnd.Interfaces;
using BackEnd.Models.AuthModels;
using BackEnd.Models.MailModels;
using BackEnd.Models.ResponseModel;
using BackEnd.Models.UserModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        private SecretClient secretClient;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private string SecretForKey = "123";
        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMailService mailService, IConfiguration configuration, IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _mailService = mailService;
            _configuration = configuration;
            this._mapper = mapper;
            //secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("KeyVault:Url")), new DefaultAzureCredential());
            //KeyVaultSecret secret = secretClient.GetSecret(_configuration.GetValue<string>("KeyVault:Secrets:AuthKey"));
            //SecretForKey = secret.Value;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userEmailExists = await userManager.FindByEmailAsync(model.Email);
            var userNameExists = await userManager.FindByNameAsync(model.UserName);
            if (userEmailExists != null || userNameExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Utente già registrato!" });


            model.UserName = model.UserName.Replace(" ", "_");

            ApplicationUser user = _mapper.Map<ApplicationUser>(model);
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await userManager.CreateAsync(user, model.Password);

            if(model.Role == "Agency")
            {
                ApplicationUser newUser = await userManager.FindByEmailAsync(user.Email);
                newUser.AgencyId = newUser.Id;
                await userManager.UpdateAsync(newUser);
            }
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

            var roleResult = await userManager.AddToRoleAsync(user, model.Role);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"https://www.amministrazionethinkhome.it/#/email-confirmation/{user.Email}/{token}";
            //Url.Action("ConfirmEmail", "Email", new { token, email = user.Email }, Request.Scheme);
            MailRequest mailRequest = new MailRequest()
            {
                ToEmail = user.Email,
                Subject = "Conferma la tua email",
                Body = $"Per attivare le tue credenziali <a href='{confirmationLink}'>clicca qui</a>"
            };
            await _mailService.SendEmailAsync(mailRequest);

            return Ok(new AuthResponseModel { Status = "Success", Message = "User created successfully!" });
        }

        //[HttpPost]
        //[Route(nameof(RegisterAdmin))]
        //public async Task<IActionResult> RegisterAdmin(RegisterModel model)
        //{
        //    model.UserName = model.UserName.Replace(" ", "_");

        //    var userExists = await userManager.FindByNameAsync(model.UserName);
        //    if (userExists != null)
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "User already exists!" });

        //    ApplicationUser user = _mapper.Map<ApplicationUser>(model);
        //    user.SecurityStamp = Guid.NewGuid().ToString();

        //    var result = await userManager.CreateAsync(user, model.Password);
        //    if (!result.Succeeded)
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel { Status = "Error", Message = "User creation failed! Please check user details and try again." });
        //    if (!await roleManager.RoleExistsAsync("Admin"))
        //        await roleManager.CreateAsync(new IdentityRole("Admin"));
        //    if (!await roleManager.RoleExistsAsync("User"))
        //        await roleManager.CreateAsync(new IdentityRole("User"));
        //    if (await roleManager.RoleExistsAsync("Admin"))
        //    {
        //        await userManager.AddToRoleAsync(user, "Admin");
        //    }
        //    return Ok(new AuthResponseModel { Status = "Success", Message = "User created successfully!" });
        //}


        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login(LoginModel model)
        {
            model.Email = model.Email.Replace(" ", "_");
            var user = await userManager.FindByEmailAsync(model.Email);
            var pass = await userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                string role = userRoles.Contains("Admin") ? "Admin" : userRoles.Contains("Agency") ? "Agenzia" : userRoles.Contains("Agent") ? "Agente" : userRoles.FirstOrDefault() ?? "";
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, role),
                };
                
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretForKey));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    expires: DateTime.Now.AddDays(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                LoginResponse result = new LoginResponse()
                {
                    Id = user.Id,
                    AgencyId = user.AgencyId ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Password = "",
                    Role = role,
                    Color = user.Color,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                };

                return Ok(result);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Utente non autorizzato" });
        }

        [HttpPost]
        [Route(nameof(VerifyToken))]
        public async Task<IActionResult> VerifyToken(TokenVerificationModel api_token)
        {
            try
            {
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

                    ClockSkew = TimeSpan.Zero // Imposta lo skew dell'orologio a zero per evitare eventuali problemi di sincronizzazione
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(api_token.api_token, validationParameters, out validatedToken);
                string? email;
                if (principal.Identity.IsAuthenticated)
                {
                    email = principal.Claims.First().Value;
                    string role = principal.Claims.ElementAt(2).Value;
                    var user = await userManager.FindByEmailAsync(email);
                    var userRoles = await userManager.GetRolesAsync(user);
                    LoginResponse result = new LoginResponse()
                    {
                        Id = user.Id,
                        AgencyId = user.AgencyId ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Password = "",
                        Role = role,
                        Token = api_token.api_token,
                        Color = user.Color
                    };

                    return Ok(result);
                }

                return Unauthorized();
            }
            catch (SecurityTokenException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(nameof(ConfirmCredentials))]
        public async Task<IActionResult> ConfirmCredentials(CredentialConfirmationModel credentials)
        {
            var user = await userManager.FindByEmailAsync(credentials.Email);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "User not exists!" });

            var result = await userManager.ConfirmEmailAsync(user, credentials.Token);

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                var updateResult = await userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                    return Ok();
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Non è stato possibile confermare le credenziali" });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseModel() { Status = "Error", Message = "Token non valido" });
        }

        [HttpGet]
        [Route(nameof(KeyGen))]
        public async Task<IActionResult> KeyGen()
        {
            byte[] keyBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            string secretKey = Convert.ToBase64String(keyBytes);
            return Ok(secretKey);
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
                    result.Role= roles.First() == "Agency" ? result.Role = "Agenzia"
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
