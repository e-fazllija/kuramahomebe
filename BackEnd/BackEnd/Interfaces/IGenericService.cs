using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces
{
    public interface IGenericService
    {
        Task<HomeDetailsModel> GetHomeDetails();
        Task<AdminHomeDetailsModel> GetAdminHomeDetails(string? agencyId);
    }
}
