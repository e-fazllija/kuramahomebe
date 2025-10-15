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

// Configurazione DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureServices();
builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("PaginationOptions"));
builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("MailOptions"));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddCors();

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// CONFIGURA JWT SOLO SE KEYVAULT È PRESENTE
var keyVaultUrl = builder.Configuration.GetSection("KeyVault:Url").Value;
var authKeySecret = builder.Configuration.GetSection("KeyVault:Secrets:AuthKey").Value;

if (!string.IsNullOrEmpty(keyVaultUrl) && !string.IsNullOrEmpty(authKeySecret))
{
    builder.ConfigureJwt(keyVaultUrl, authKeySecret);
}
else
{
    // Configurazione alternativa per sviluppo senza KeyVault
    Console.WriteLine("KeyVault non configurato, JWT sarà configurato con valori di default");
    // Se hai un metodo alternativo per configurare JWT, chiamalo qui
    // builder.ConfigureJwtForDevelopment();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors(options => options.WithOrigins("*").AllowAnyMethod().AllowAnyHeader());

// Seed locations data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await LocationDataSeeder.SeedLocations(context);
}

app.Run();