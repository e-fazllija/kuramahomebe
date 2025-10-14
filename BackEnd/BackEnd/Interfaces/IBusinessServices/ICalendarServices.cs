using BackEnd.Entities;
using BackEnd.Models.CalendarModels;
using BackEnd.Models.OutputModels;

namespace BackEnd.Interfaces.IBusinessServices
{
    public interface ICalendarServices
    {
        Task<CalendarSelectModel> Create(CalendarCreateModel dto);
        Task<ListViewModel<CalendarSelectModel>> Get(string? agencyId, string? agentId, char? fromName, char? toName);
        Task<CalendarCreateViewModel> GetToInsert(string agencyId);
        Task<CalendarSearchModel> GetSearchItems(string userId, string? agencyId);
        Task<CalendarSelectModel> Update(CalendarUpdateModel dto);
        Task<CalendarSelectModel> GetById(int id);
        Task<Calendar> Delete(int id);
    }
}
