using BackEnd.Entities;
using BackEnd.Models.CalendarModels;

namespace BackEnd.Profiles
{
    public class CalendarProfile : AutoMapper.Profile
    {
        public CalendarProfile()
        {
            CreateMap<Calendar, CalendarCreateModel>();
            CreateMap<Calendar, CalendarUpdateModel>();
            CreateMap<Calendar, CalendarSelectModel>();
            CreateMap<CalendarSelectModel, CalendarUpdateModel>();
            CreateMap<CalendarUpdateModel, CalendarSelectModel>();

            CreateMap<CalendarCreateModel, Calendar>();
            CreateMap<CalendarUpdateModel, Calendar>();
            CreateMap<CalendarSelectModel, Calendar>();

        }
    }
}
