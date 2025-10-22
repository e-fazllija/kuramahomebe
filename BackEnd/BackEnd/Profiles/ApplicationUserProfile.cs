using BackEnd.Entities;
using BackEnd.Models.AuthModels;
using BackEnd.Models.UserModel;

namespace BackEnd.Profiles
{
    public class ApplicationUserProfile: AutoMapper.Profile
    {
        public ApplicationUserProfile()
        {
            CreateMap<ApplicationUser, RegisterModel>();
            CreateMap<RegisterModel, ApplicationUser>();

            CreateMap<ApplicationUser, UserUpdateModel>();
            
            CreateMap<UserUpdateModel, ApplicationUser>()
                .ForMember(dest => dest.UserType, opt => opt.Ignore()) // UserType non modificabile, non esiste in UpdateModel
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<ApplicationUser, UserSelectModel>();
            CreateMap<UserSelectModel, ApplicationUser>();
        }
    }
}
