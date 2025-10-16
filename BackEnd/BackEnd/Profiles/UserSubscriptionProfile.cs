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
            CreateMap<UserSubscription, UserSubscriptionSelectModel>();

            // CreateModel -> Entity
            CreateMap<UserSubscriptionCreateModel, UserSubscription>();

            // UpdateModel -> Entity
            CreateMap<UserSubscriptionUpdateModel, UserSubscription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

