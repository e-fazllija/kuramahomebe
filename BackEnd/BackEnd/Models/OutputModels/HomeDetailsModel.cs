using BackEnd.Models.RealEstatePropertyModels;

namespace BackEnd.Models.OutputModels
{
    public class HomeDetailsModel
    {
        public List<RealEstatePropertySelectModel> RealEstatePropertiesInHome { get; set; } = new List<RealEstatePropertySelectModel>();
        public RealEstatePropertySelectModel RealEstatePropertiesHighlighted { get; set; } = new RealEstatePropertySelectModel();
    }
}
