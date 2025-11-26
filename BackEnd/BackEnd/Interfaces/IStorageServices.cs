using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces
{
    public interface IStorageServices
    {
        Task<string> UploadFile(Stream file, string fileName);
        Task<bool> DeleteFile(string fileName);
        Task<FileResponse> DownloadFile(string filename);
        Task<string> CreateAuthZip(string operaId);
        string GetFileNameFromUrl(string url);
        
        /// <summary>
        /// Genera un URL con Shared Access Signature (SAS) per accesso temporaneo a file privati
        /// </summary>
        /// <param name="fileName">Nome del file nel blob storage</param>
        /// <param name="expirationMinutes">Durata validità del token in minuti (default: 60)</param>
        /// <returns>URL completo con SAS token per accesso temporaneo</returns>
        string GenerateSasUrl(string fileName, int expirationMinutes = 60);
    }
}
