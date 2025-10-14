using BackEnd.Models.LocationModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface ILocationServices
    {
        Task<LocationSelectModel> Create(LocationCreateModel dto);
        Task<LocationSelectModel> Update(LocationUpdateModel dto);
        Task<LocationSelectModel> GetById(int id);
        Task<ListViewModel<LocationSelectModel>> Get(int currentPage, string? filterRequest, string? city);
        Task<List<LocationSelectModel>> GetAll();
        Task<List<LocationGroupedModel>> GetGroupedByCity();
        Task Delete(int id);
        Task<bool> SeedLocations();
    }
} 