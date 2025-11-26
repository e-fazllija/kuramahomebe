using System.Collections.Generic;

namespace BackEnd.Models.Options
{
    public class KeyVaultOptions
    {
        public string? Url { get; set; }
        public Dictionary<string, string>? Secrets { get; set; }
        public int CacheExpirationMinutes { get; set; } = 30;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Url);
    }
}


