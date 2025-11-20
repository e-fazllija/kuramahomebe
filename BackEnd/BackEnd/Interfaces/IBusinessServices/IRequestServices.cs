using BackEnd.Entities;
using BackEnd.Models.RequestModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IRequestServices
    {
        Task<RequestSelectModel> Create(RequestCreateModel dto);
        Task<ListViewModel<RequestSelectModel>> Get(int currentPage, string? filterRequest, char? fromName, char? toName, string? userId);
        Task<ListViewModel<RequestListModel>> GetList(int currentPage, string? filterRequest, char? fromName, char? toName, string? userId);
        Task<List<RequestListModel>> GetForExportAsync(RequestExportModel filters, string userId);
        Task<ListViewModel<RequestSelectModel>> GetCustomerRequests(int customerId);
        Task<RequestSelectModel> Update(RequestUpdateModel dto);
        Task<RequestSelectModel> GetById(int id);
        Task<Request> Delete(int id);
        Task<RequestDeleteConstraintsModel> CanDelete(int id);
    }
}
