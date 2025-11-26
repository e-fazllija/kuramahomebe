using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;

namespace BackEnd.Services.Repositories
{
    public class CalendarRepository : GenericRepository<Calendar>, ICalendarRepository
    {
        public CalendarRepository(AppDbContext context) : base(context) { }
    }
}
