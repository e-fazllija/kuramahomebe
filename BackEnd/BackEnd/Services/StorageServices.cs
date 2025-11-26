using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO.Compression;
using BackEnd.Interfaces;
using BackEnd.Models.OutputModels;

namespace BackEnd.Services
{
    /// <summary>
    /// Servizio per la gestione dello storage dei documenti privati.
    /// Utilizza un container blob con accesso privato che richiede autenticazione.
    /// Per le immagini delle proprietà pubbliche, usare IPropertyStorageService.
    /// </summary>
    public class StorageServices : IStorageServices
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StorageServices> _logger;
        private readonly IKeyVaultSecretProvider _secretProvider;
        private string blobstorageconnection;
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;
        
        public StorageServices(IConfiguration configuration, ILogger<StorageServices> logger, IKeyVaultSecretProvider secretProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _secretProvider = secretProvider;

            var secretName = _configuration.GetValue<string>("KeyVault:Secrets:StorageConnectionString");
            var fallbackKey = "Storage:LocalConnectionString";
            blobstorageconnection = _secretProvider.GetSecret(secretName, fallbackKey)
                ?? _configuration.GetValue<string>(fallbackKey)
                ?? throw new InvalidOperationException("Storage connection string not configured.");
            cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            blobClient = cloudStorageAccount.CreateCloudBlobClient();
            
            // Container per documenti privati (richiede autenticazione)
            container = blobClient.GetContainerReference(_configuration.GetValue<string>("Storage:BlobContainerName"));
            
            // Assicura che il container esista con permessi privati
            InitializeContainerAsync().Wait();
        }

        /// <summary>
        /// Inizializza il container con permessi privati (accesso solo autenticato)
        /// </summary>
        private async Task InitializeContainerAsync()
        {
            try
            {
                // Crea il container se non esiste
                await container.CreateIfNotExistsAsync();

                // Imposta i permessi di accesso privato (nessun accesso anonimo)
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                };

                await container.SetPermissionsAsync(permissions);
                
                _logger.LogInformation($"Container documenti '{container.Name}' inizializzato con accesso privato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'inizializzazione del container documenti: {ex.Message}");
                throw;
            }
        }
        public async Task<string> UploadFile(Stream file, string fileName)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                await blockBlob.UploadFromStreamAsync(file);

                return blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteFile(string fileName)
        {
            try
            {
                var blob = container.GetBlobReference(fileName);
                await blob.DeleteIfExistsAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<FileResponse> DownloadFile(string fileName)
        {
            try
            {
                CloudBlockBlob blob;
                await using (MemoryStream memoryStream = new MemoryStream())
                {
                    blob = container.GetBlockBlobReference(fileName);
                    await blob.DownloadToStreamAsync(memoryStream);
                }
                Stream blobStream = blob.OpenReadAsync().Result;
                return new FileResponse(blobStream, blob.Properties.ContentType, blob.Name);

            }
            catch (Exception e)
            {
                return null;
            }
        }


        public async Task<string> CreateAuthZip(string operaId)
        {
            try
            {
                string BasePath = "Opere/" + operaId + "/";
                string AuthBasePath = "Auth/" + operaId + "/";

                CloudBlockBlob zipblob = container.GetBlockBlobReference(AuthBasePath + "Auth_" + operaId + ".zip");
                CloudBlobStream zipstream = await zipblob.OpenWriteAsync();
                using (ZipArchive zipArchive = new ZipArchive(zipstream, ZipArchiveMode.Create))
                {
                    CloudBlobDirectory dira = container.GetDirectoryReference(BasePath);
                    BlobResultSegment rootDirFolders = dira.ListBlobsSegmentedAsync(true, BlobListingDetails.Metadata, null, null, null, null).Result;
                    foreach (IListBlobItem blob in rootDirFolders.Results)
                    {
                        Console.WriteLine(blob.Uri.ToString());
                        CloudBlockBlob cbb = (CloudBlockBlob)blob;
                        using (Stream blobstream = await cbb.OpenReadAsync())
                        {
                            ZipArchiveEntry entry = zipArchive.CreateEntry(cbb.Name, CompressionLevel.Optimal);
                            using (var innerFile = entry.Open())
                            {
                                await blobstream.CopyToAsync(innerFile);
                            }
                        }
                    }
                }
                return zipblob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string GetFileNameFromUrl(string url)
        {
            string container = _configuration.GetValue<string>("Storage:BlobContainerName");
            string urlBlob = _configuration.GetValue<string>("Storage:Url");
            var substring = url.Replace(urlBlob + container + "/", "");
            return substring;
        }

        /// <summary>
        /// Genera un URL con Shared Access Signature (SAS) per accesso temporaneo a file privati
        /// </summary>
        public string GenerateSasUrl(string fileName, int expirationMinutes = 60)
        {
            try
            {
                var blob = container.GetBlockBlobReference(fileName);

                // Crea una policy SAS con permessi di lettura
                var sasConstraints = new SharedAccessBlobPolicy
                {
                    SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5), // 5 minuti di buffer per differenze di clock
                    SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Genera il token SAS
                var sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

                // Restituisce l'URL completo con il token SAS
                return blob.Uri + sasBlobToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante la generazione del SAS token per '{fileName}': {ex.Message}");
                throw new Exception("Si è verificato un errore durante la generazione dell'accesso al file.");
            }
        }

    }


}
