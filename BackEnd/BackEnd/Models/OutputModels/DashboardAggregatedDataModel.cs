using BackEnd.Models.CalendarModels;
using BackEnd.Models.CustomerModels;
using BackEnd.Models.RealEstatePropertyModels;
using BackEnd.Models.RequestModels;
using BackEnd.Models.UserModel;

namespace BackEnd.Models.OutputModels
{
    public class DashboardAggregatedDataModel
    {
        // Dati aggregati esistenti
        public RealEstatePropertyHomeDetails RealEstatePropertyHomeDetails { get; set; } = new RealEstatePropertyHomeDetails();
        public RequestHomeDetails RequestHomeDetails { get; set; } = new RequestHomeDetails();
        public int TotalCustomers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalAgents { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalConfirmedAppointments { get; set; }

        // Liste complete di dati
        public List<RealEstatePropertyListModel> AvailableProperties { get; set; } = new List<RealEstatePropertyListModel>();
        public List<RealEstatePropertyListModel> SoldProperties { get; set; } = new List<RealEstatePropertyListModel>();
        public List<UserSelectModel> Agents { get; set; } = new List<UserSelectModel>();
        public List<UserSelectModel> Agencies { get; set; } = new List<UserSelectModel>();
        public List<CustomerSelectModel> Customers { get; set; } = new List<CustomerSelectModel>();
        public List<CalendarSelectModel> CalendarEvents { get; set; } = new List<CalendarSelectModel>();
        public List<RequestSelectModel> Requests { get; set; } = new List<RequestSelectModel>();
    }
}

