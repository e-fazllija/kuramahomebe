using BackEnd.Entities;
using BackEnd.Models.RealEstatePropertyModels;
using System.Linq;

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

            // Mapping per Dashboard - i nomi delle proprietà corrispondono all'entità
            CreateMap<RealEstateProperty, RealEstatePropertyListModel>()
                .ForMember(dest => dest.FirstPhotoUrl, opt => opt.MapFrom(src => 
                    src.Photos != null && src.Photos.Any() 
                        ? src.Photos.OrderBy(p => p.Position).FirstOrDefault().Url 
                        : null))
                .ForMember(dest => dest.AgencyId, opt => opt.MapFrom(src => src.User != null ? src.User.AdminId : null))
                .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.UserId));

        }
    }
}
