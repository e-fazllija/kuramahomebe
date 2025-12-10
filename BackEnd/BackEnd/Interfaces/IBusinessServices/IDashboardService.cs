using BackEnd.Models.OutputModels;

using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IDashboardService
    {
        Task<MapDataModel> GetMapData(string? userId, string? agencyId, int? year);
        Task<Widget3DataModel> GetWidget3Data(string? userId, string? agencyId, int? year);
        Task<TopAgenciesDataModel> GetTopAgenciesData(string? userId, int? year, string? sortBy = null, string? sortOrder = "desc");
        Task<TopAgentsDataModel> GetTopAgentsData(string? userId, int? year, string? sortBy = null, string? sortOrder = "desc");
        Task<TopZonesDataModel> GetTopZonesData(string? userId);
        Task<TopTypologiesDataModel> GetTopTypologiesData(string? userId);
        Task<TopEarningsDataModel> GetTopEarningsData(string? userId, int? year);
        Task<AnalyticsDataModel> GetAnalyticsData(string? userId, int year, string? agencyId = null);
        Task<ExpiringAssignmentsDataModel> GetExpiringAssignments(string? userId, int? daysThreshold = 30);
    }
}


