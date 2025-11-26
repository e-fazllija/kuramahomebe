using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces
{
    public interface IGenericService
    {
        Task<HomeDetailsModel> GetHomeDetails();
        Task<AdminHomeDetailsModel> GetAdminHomeDetails(string? agencyId);
        Task<DashboardAggregatedDataModel> GetDashboardAggregatedData(string? agencyId, int? year);
    }
}
