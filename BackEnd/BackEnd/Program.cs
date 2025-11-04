using BackEnd;
using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Models.Options;
using BackEnd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureServices();
builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("PaginationOptions"));
builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("MailOptions"));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddCors();

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

// CORS deve essere configurato PRIMA di Authentication e Authorization
app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed locations data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await LocationDataSeeder.SeedLocations(context);
}

// Seed roles data
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roleNames = { "Admin", "Agency", "Agent", "User" };
    
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var role = new IdentityRole(roleName);
            await roleManager.CreateAsync(role);
            Console.WriteLine($"Ruolo '{roleName}' creato con successo.");
        }
        else
        {
            Console.WriteLine($"Ruolo '{roleName}' già esistente, skip.");
        }
    }
    
    Console.WriteLine("Seed dei ruoli completato.");
}

// Seed subscription plans data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SubscriptionPlanSeeder.SeedSubscriptionPlans(context);
}

// Seed test data (solo in Development e se abilitato nella configurazione)
if (app.Environment.IsDevelopment())
{
    var seedTestData = builder.Configuration.GetValue<bool>("SeedTestData", false);
    if (seedTestData)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            var seeder = new TestDataSeeder(context, userManager, roleManager);
            await seeder.SeedTestData();
        }
    }
}

app.Run();