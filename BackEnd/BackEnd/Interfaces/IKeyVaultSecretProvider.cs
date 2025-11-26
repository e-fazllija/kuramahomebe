using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Interfaces
{
    public interface IKeyVaultSecretProvider
    {
        Task<string?> GetSecretAsync(string? secretName, string? fallbackConfigurationKey = null, CancellationToken cancellationToken = default);
        string? GetSecret(string? secretName, string? fallbackConfigurationKey = null);
    }
}


