using BackEnd.Entities;
using BackEnd.Models.RealEstatePropertyModels;

namespace BackEnd.Profiles
{
    public class RealEstatePropertyProfile : AutoMapper.Profile
    {
        public RealEstatePropertyProfile()
        {
            CreateMap<RealEstateProperty, RealEstatePropertyCreateModel>();
            CreateMap<RealEstateProperty, RealEstatePropertyUpdateModel>();
            CreateMap<RealEstateProperty, RealEstatePropertySelectModel>();
            CreateMap<RealEstatePropertySelectModel, RealEstatePropertyUpdateModel>();
            CreateMap<RealEstatePropertyUpdateModel, RealEstatePropertySelectModel>();

            CreateMap<RealEstatePropertyCreateModel, RealEstateProperty>();
            CreateMap<RealEstatePropertyUpdateModel, RealEstateProperty>();
            CreateMap<RealEstatePropertySelectModel, RealEstateProperty>();

        }
    }
}
