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
            CreateMap<UserUpdateModel, ApplicationUser>();

            CreateMap<ApplicationUser, UserSelectModel>();
            CreateMap<UserSelectModel, ApplicationUser>();
        }
    }
}
