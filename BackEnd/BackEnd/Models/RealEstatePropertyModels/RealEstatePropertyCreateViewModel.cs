using BackEnd.Models.CustomerModels;
using BackEnd.Models.UserModel;

namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyCreateViewModel
    {
        public List<CustomerSelectModel> Customers {  get; set; } =  new List<CustomerSelectModel>();
        public List<UserSelectModel> Agents {  get; set; } =  new List<UserSelectModel>();
    }
}
