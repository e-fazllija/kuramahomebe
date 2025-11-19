using BackEnd.Entities;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface ICustomerServices
    {
        Task<CustomerSelectModel> Create(CustomerCreateModel dto);
        Task<ListViewModel<CustomerSelectModel>> Get(string? userId, string? filterRequest, char? fromName, char? toName);
        Task<List<CustomerSelectModel>> GetForExportAsync(CustomerExportModel filters, string userId);
        Task<CustomerSelectModel> Update(CustomerUpdateModel dto);
        Task<CustomerSelectModel> GetById(int id);
        Task<Customer> Delete(int id);
    }
}
