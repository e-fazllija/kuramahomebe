using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.UserSubscriptionModels;

namespace BackEnd.Profiles
{
    public class UserSubscriptionProfile : Profile
    {
        public UserSubscriptionProfile()
        {
            // Entity -> SelectModel
            CreateMap<UserSubscription, UserSubscriptionSelectModel>()
                .ForMember(dest => dest.SubscriptionPlan, opt => opt.MapFrom(src => src.SubscriptionPlan))
                .ForMember(dest => dest.LastPayment, opt => opt.MapFrom(src => src.LastPayment));

            // CreateModel -> Entity
            CreateMap<UserSubscriptionCreateModel, UserSubscription>();

            // UpdateModel -> Entity
            CreateMap<UserSubscriptionUpdateModel, UserSubscription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

