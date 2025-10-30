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

            CreateMap<ApplicationUser, UserCreateModel>();
            CreateMap<UserCreateModel, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email.Replace(" ", "_"))) // Genera UserName da Email
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore()) // Gestito manualmente nel controller
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province)) // Mappa Province
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color)) // Mappa Color esplicitamente
                .ForMember(dest => dest.Agency, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.RealEstateProperties, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Payments, opt => opt.Ignore()); // Navigation property


            CreateMap<ApplicationUser, UserUpdateModel>();
            
            CreateMap<UserUpdateModel, ApplicationUser>()
                .ForMember(dest => dest.UserType, opt => opt.Ignore()) // UserType non modificabile, non esiste in UpdateModel
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province)) // Mappa Province
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color)) // Mappa Color esplicitamente
                .ForMember(dest => dest.Agency, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.RealEstateProperties, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Payments, opt => opt.Ignore()); // Navigation property

            CreateMap<ApplicationUser, UserSelectModel>()
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province)) // Mappa Province esplicitamente
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color)); // Mappa Color esplicitamente
            CreateMap<UserSelectModel, ApplicationUser>();
        }
    }
}
