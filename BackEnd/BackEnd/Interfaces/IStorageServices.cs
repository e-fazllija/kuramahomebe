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
    }
}
