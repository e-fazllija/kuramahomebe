using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BackEnd.Services
{
    public static class DatabaseStartup
    {

        public static readonly ILoggerFactory ConsoleLogFactory
                        = LoggerFactory.Create(builder => { builder.AddConsole(); });
        /// <summary>
        /// It is used to configure the connection properties for the database
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static void ConfigureDatabase(this WebApplicationBuilder builder, string? keyVaultUrl, string? secretName)
        {
            // For development or when KeyVault parameters are null, use connection string from appsettings
            if (/*builder.Environment.IsDevelopment() ||*/ string.IsNullOrEmpty(keyVaultUrl) || string.IsNullOrEmpty(secretName))
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.");
                }

                builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            }
            else
            {
                // For production, use KeyVault
                SecretClient client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                KeyVaultSecret secret = client.GetSecret(secretName);

                builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(
                        secret.Value);                          

                });
            }
        }
    }
}