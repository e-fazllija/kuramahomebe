using BackEnd.Entities;
using BackEnd.Models.RealEstatePropertyPhotoModels;

namespace BackEnd.Profiles
{
    public class RealEstatePropertyPhotoProfile : AutoMapper.Profile
    {
        public RealEstatePropertyPhotoProfile()
        {
            CreateMap<RealEstatePropertyPhoto, RealEstatePropertyPhotoCreateModel>();
            CreateMap<RealEstatePropertyPhoto, RealEstatePropertyPhotoUpdateModel>();
            CreateMap<RealEstatePropertyPhoto, RealEstatePropertyPhotoSelectModel>();
            CreateMap<RealEstatePropertyPhotoSelectModel, RealEstatePropertyPhotoUpdateModel>();
            CreateMap<RealEstatePropertyPhotoUpdateModel, RealEstatePropertyPhotoSelectModel>();

            CreateMap<RealEstatePropertyPhotoCreateModel, RealEstatePropertyPhoto>();
            CreateMap<RealEstatePropertyPhotoUpdateModel, RealEstatePropertyPhoto>();
            CreateMap<RealEstatePropertyPhotoSelectModel, RealEstatePropertyPhoto>();

        }
    }
}
