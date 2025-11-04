using BackEnd.Models.OutputModels;
using BackEnd.Models.LocationModels;

namespace BackEnd.Interfaces
{
    public interface IGenericService
    {
        Task<HomeDetailsModel> GetHomeDetails();
        Task<AdminHomeDetailsModel> GetAdminHomeDetails(string? agencyId);
        Task<DashboardAggregatedDataModel> GetDashboardAggregatedData(string? agencyId, int? year);
    }
}
