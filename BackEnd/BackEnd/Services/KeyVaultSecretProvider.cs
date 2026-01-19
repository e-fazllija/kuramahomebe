using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BackEnd.Interfaces;
using BackEnd.Models.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BackEnd.Services
{
    public class KeyVaultSecretProvider : IKeyVaultSecretProvider
    {
        private readonly SecretClient? _secretClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<KeyVaultSecretProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _cacheDuration;
        private readonly bool _isEnabled;

        public KeyVaultSecretProvider(
            IOptions<KeyVaultOptions> options,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<KeyVaultSecretProvider> logger)
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;

            var keyVaultOptions = options.Value;
            _cacheDuration = TimeSpan.FromMinutes(
                keyVaultOptions.CacheExpirationMinutes > 0
                    ? keyVaultOptions.CacheExpirationMinutes
                    : 30);

            if (!string.IsNullOrWhiteSpace(keyVaultOptions.Url))
            {
                _secretClient = new SecretClient(new Uri(keyVaultOptions.Url), new DefaultAzureCredential());
                _isEnabled = true;
            }
        }

        public async Task<string?> GetSecretAsync(string? secretName, string? fallbackConfigurationKey = null, CancellationToken cancellationToken = default)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(secretName))
            {
                return GetFallbackValue(fallbackConfigurationKey);
            }

            return await _cache.GetOrCreateAsync(secretName, async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = _cacheDuration;

                try
                {
                    var response = await _secretClient!.GetSecretAsync(secretName, cancellationToken: cancellationToken);
                    return response.Value.Value;
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogError(ex, "Errore nel recupero della secret '{SecretName}' da KeyVault", secretName);
                    var fallbackValue = GetFallbackValue(fallbackConfigurationKey);
                    if (!string.IsNullOrWhiteSpace(fallbackValue))
                    {
                        return fallbackValue;
                    }
                    throw;
                }
                catch (AuthenticationFailedException ex)
                {
                    _logger.LogError(ex, "Autenticazione fallita verso KeyVault");
                    var fallbackValue = GetFallbackValue(fallbackConfigurationKey);
                    if (!string.IsNullOrWhiteSpace(fallbackValue))
                    {
                        return fallbackValue;
                    }
                    throw;
                }
            });
        }

        public string? GetSecret(string? secretName, string? fallbackConfigurationKey = null)
        {
            return GetSecretAsync(secretName, fallbackConfigurationKey).GetAwaiter().GetResult();
        }

        private string? GetFallbackValue(string? fallbackKey)
        {
            if (string.IsNullOrWhiteSpace(fallbackKey))
            {
                return null;
            }

            return _configuration[fallbackKey];
        }
    }
}


