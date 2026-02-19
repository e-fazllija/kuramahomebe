using BackEnd;
using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Hubs;
using BackEnd.Models.Options;
using BackEnd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Configura limiti upload file (multipart form) - necessario per file fino a 35 MB
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 35 * 1024 * 1024; // 35 MB (leggermente oltre il limite 30 MB per overhead)
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Accetta camelCase dal frontend (keyword, province, city...) nella deserializzazione delle richieste
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureServices();
builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("PaginationOptions"));
builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("MailOptions"));
builder.Services.Configure<KeyVaultOptions>(builder.Configuration.GetSection("KeyVault"));
builder.Services.AddAutoMapper(cfg => { }, typeof(BackEnd.Profiles.PaymentProfile).Assembly);
builder.Services.AddCors();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        // Usa PascalCase per coerenza con i controller REST
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
        options.PayloadSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// For Identity - DOPO JWT per non interferire
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var keyVaultUrl = builder.Configuration.GetSection("KeyVault:Url").Value;
var authKeySecret = builder.Configuration.GetSection("KeyVault:Secrets:AuthKey").Value;
var dbSecret = builder.Configuration.GetSection("KeyVault:Secrets:DbConnectionString").Value;

builder.ConfigureDatabase(keyVaultUrl, dbSecret);
if (!string.IsNullOrEmpty(keyVaultUrl) && !string.IsNullOrEmpty(authKeySecret))
{
    builder.ConfigureJwt(keyVaultUrl, authKeySecret);
    Console.WriteLine("JWT configurato con KeyVault");
}
else
{    
    builder.ConfigureJwtForDevelopment();
    Console.WriteLine("KeyVault non configurato, JWT configurato con chiave di sviluppo");
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS: AllowCredentials richiesto da SignalR WebSocket
app.UseCors(options => options
    .SetIsOriginAllowed(_ => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

#region "Seed Initial Data"
// Seed roles data
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    string[] roleNames = { "Admin", "Agency", "Agent", "User" };

//    foreach (var roleName in roleNames)
//    {
//        var roleExist = await roleManager.RoleExistsAsync(roleName);
//        if (!roleExist)
//        {
//            var role = new IdentityRole(roleName);
//            await roleManager.CreateAsync(role);
//            Console.WriteLine($"Ruolo '{roleName}' creato con successo.");
//        }
//        else
//        {
//            Console.WriteLine($"Ruolo '{roleName}' già esistente, skip.");
//        }
//    }

//    Console.WriteLine("Seed dei ruoli completato.");
//}

////Assicura che il piano Free esista sempre (necessario per i trial)
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    await SubscriptionPlanSeeder.EnsureFreePlanExists(context);
//}

////Seed test data (solo in Development e se abilitato nella configurazione)
//if (app.Environment.IsDevelopment())
//{
//    var seedTestData = builder.Configuration.GetValue<bool>("SeedTestData", true);
//    if (seedTestData)
//    {
//        using (var scope = app.Services.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//            //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//            //var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//            //var seeder = new TestDataSeeder(context, userManager, roleManager);
//            //await seeder.SeedTestData();

//            await SubscriptionPlanSeeder.SeedSubscriptionPlans(context);

//        }
//    }
//}
#endregion

app.Run();