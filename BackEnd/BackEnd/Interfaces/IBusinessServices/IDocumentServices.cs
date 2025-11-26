using BackEnd.Entities;
using BackEnd.Models.CalendarModels;
using BackEnd.Models.InputModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IDocumentServices
    {
        Task<Documentation> UploadDocument(SendFileModel dto);
        Task DeleteDocument(int id);


    }
}
