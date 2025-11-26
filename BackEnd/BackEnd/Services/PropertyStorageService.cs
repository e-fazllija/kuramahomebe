using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BackEnd.Interfaces;
using BackEnd.Models.OutputModels;

namespace BackEnd.Services
{
    /// <summary>
    /// Servizio dedicato alla gestione dello storage per le immagini delle proprietà immobiliari.
    /// Utilizza un container blob con accesso pubblico/anonimo per consentire la visualizzazione
    /// delle immagini senza autenticazione.
    /// </summary>
    public class PropertyStorageService : IPropertyStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _blobStorageConnection;
        private readonly CloudStorageAccount _cloudStorageAccount;
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _container;
        private readonly ILogger<PropertyStorageService> _logger;
        private readonly IKeyVaultSecretProvider _secretProvider;

        public PropertyStorageService(IConfiguration configuration, ILogger<PropertyStorageService> logger, IKeyVaultSecretProvider secretProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _secretProvider = secretProvider;

            var secretName = _configuration.GetValue<string>("KeyVault:Secrets:StorageConnectionString");
            var fallbackKey = "Storage:LocalConnectionString";
            _blobStorageConnection = _secretProvider.GetSecret(secretName, fallbackKey)
                ?? _configuration.GetValue<string>(fallbackKey)
                ?? throw new InvalidOperationException("Storage connection string not configured.");
            
            _cloudStorageAccount = CloudStorageAccount.Parse(_blobStorageConnection);
            _blobClient = _cloudStorageAccount.CreateCloudBlobClient();
            
            // Container dedicato per le immagini delle proprietà (pubblico)
            var containerName = _configuration.GetValue<string>("Storage:PropertiesBlobContainerName");
            _container = _blobClient.GetContainerReference(containerName);
            
            // Assicura che il container esista e sia pubblico per l'accesso anonimo
            InitializeContainerAsync().Wait();
        }

        /// <summary>
        /// Inizializza il container e imposta i permessi di accesso pubblico/anonimo per i blob
        /// </summary>
        private async Task InitializeContainerAsync()
        {
            try
            {
                // Crea il container se non esiste
                await _container.CreateIfNotExistsAsync();

                // Imposta i permessi di accesso pubblico per i blob (non per il container)
                // Questo permette l'accesso anonimo alle immagini tramite URL diretto
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };

                await _container.SetPermissionsAsync(permissions);
                
                _logger.LogInformation($"Container '{_container.Name}' inizializzato con accesso pubblico ai blob");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'inizializzazione del container: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadPropertyImage(Stream file, string fileName)
        {
            try
            {
                CloudBlockBlob blockBlob = _container.GetBlockBlobReference(fileName);
                
                // Imposta il content type in base all'estensione del file
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                blockBlob.Properties.ContentType = GetContentType(extension);
                
                await blockBlob.UploadFromStreamAsync(file);

                _logger.LogInformation($"Immagine caricata con successo: {fileName}");
                
                return blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'upload dell'immagine '{fileName}': {ex.Message}");
                throw new Exception($"Errore durante l'upload dell'immagine: {ex.Message}");
            }
        }

        public async Task<bool> DeletePropertyImage(string fileName)
        {
            try
            {
                var blob = _container.GetBlobReference(fileName);
                bool deleted = await blob.DeleteIfExistsAsync();
                
                if (deleted)
                {
                    _logger.LogInformation($"Immagine eliminata con successo: {fileName}");
                }
                else
                {
                    _logger.LogWarning($"Immagine non trovata per l'eliminazione: {fileName}");
                }
                
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'eliminazione dell'immagine '{fileName}': {ex.Message}");
                return false;
            }
        }

        public async Task<FileResponse> DownloadPropertyImage(string fileName)
        {
            try
            {
                CloudBlockBlob blob;
                await using (MemoryStream memoryStream = new MemoryStream())
                {
                    blob = _container.GetBlockBlobReference(fileName);
                    await blob.DownloadToStreamAsync(memoryStream);
                }
                
                Stream blobStream = blob.OpenReadAsync().Result;
                
                _logger.LogInformation($"Immagine scaricata con successo: {fileName}");
                
                return new FileResponse(blobStream, blob.Properties.ContentType, blob.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante il download dell'immagine '{fileName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Determina il Content-Type in base all'estensione del file
        /// </summary>
        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}

