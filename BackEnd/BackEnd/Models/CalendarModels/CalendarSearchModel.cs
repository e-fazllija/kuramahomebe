using BackEnd.Models.UserModel;

namespace BackEnd.Models.CalendarModels
{
    public class CalendarSearchModel
    {
        public List<UserSelectModel>? Agencies {  get; set; }
        public List<UserSelectModel> Agents { get; set; } = new List<UserSelectModel>();
    }
}
