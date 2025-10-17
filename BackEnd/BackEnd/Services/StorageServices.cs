using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO.Compression;
using BackEnd.Interfaces;
using BackEnd.Models.OutputModels;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BackEnd.Services
{
    public class StorageServices : IStorageServices
    {
        private readonly IConfiguration _configuration;
        private string blobstorageconnection;
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;
        private SecretClient secretClient;
        public StorageServices(IConfiguration configuration)
        {
            _configuration = configuration;
            //secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("KeyVault:Url")), new DefaultAzureCredential());
            //KeyVaultSecret secret = secretClient.GetSecret(_configuration.GetValue<string>("KeyVault:Secrets:StorageConnectionString"));
            blobstorageconnection = _configuration.GetValue<string>("Storage:LocalConnectionString")!;//secret.Value;
            cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            blobClient = cloudStorageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(_configuration.GetValue<string>("Storage:BlobContainerName"));
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

    }


}
