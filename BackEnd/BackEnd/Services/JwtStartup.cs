using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd.Services
{
    public static class JwtStartup
    {
        public static void ConfigureJwt(this WebApplicationBuilder builder, string keyVaultUrl, string secretName)
        {

            SecretClient client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret(secretName);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Authentication:Issuer"],
                    ValidAudience = builder.Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(secret.Value))
                };
            });

            //builder.Services.AddAuthentication("Bearer")
            //    .AddJwtBearer(options =>
            //        { 
            //            options.SaveToken = true;
            //            options.RequireHttpsMetadata = false;
            //            options.TokenValidationParameters = new()
            //            {
            //                ValidateIssuer = true,
            //                ValidateAudience = true,
            //                ValidateIssuerSigningKey = true,
            //                ValidIssuer = builder.Configuration["Authentication:Issuer"],
            //                ValidAudience = builder.Configuration["Authentication:Audience"],
            //                IssuerSigningKey = new SymmetricSecurityKey(
            //                    Encoding.ASCII.GetBytes(secret.Value))
            //            };
            //        }
            //    );
        }
    }
}