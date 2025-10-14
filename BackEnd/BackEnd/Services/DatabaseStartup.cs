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
        public static void ConfigureDatabase(this WebApplicationBuilder builder, string keyVaultUrl, string secretName)
        {
            SecretClient client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret(secretName);

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    secret.Value,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);

                    });
                #if DEBUG
                options.UseLoggerFactory(ConsoleLogFactory);
                options.EnableSensitiveDataLogging();
                #endif
            }

        );
        }
    }
}