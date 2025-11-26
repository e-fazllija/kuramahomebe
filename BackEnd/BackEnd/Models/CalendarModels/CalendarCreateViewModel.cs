using BackEnd.Models.CustomerModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RequestModels;

namespace BackEnd.Models.CalendarModels
{
    public class CalendarCreateViewModel
    {
        public List<CustomerSelectModel> Customers { get; set; } = new List<CustomerSelectModel>();
        public List<RealEstatePropertySelectModel> RealEstateProperties { get; set; } = new List<RealEstatePropertySelectModel>();
        public List<RequestSelectModel> Requests { get; set; } = new List<RequestSelectModel>();
    }
}
