using BackEnd.Entities;
using BackEnd.Models.DocumentsTabModels;
using BackEnd.Models.DocumentsTabModelModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IDocumentsTabServices
    {
        Task<DocumentsTabSelectModel> Create(DocumentsTabCreateModel dto);
        Task<DocumentsTabSelectModel> Update(DocumentsTabUpdateModel dto);
        Task<DocumentsTabSelectModel> GetById(int id);
        Task<DocumentsTab> Delete(int id);
    }
}
