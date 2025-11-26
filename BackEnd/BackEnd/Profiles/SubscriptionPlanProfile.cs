using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.SubscriptionPlanModels;

namespace BackEnd.Profiles
{
    public class SubscriptionPlanProfile : Profile
    {
        public SubscriptionPlanProfile()
        {
            // Entity -> SelectModel
            CreateMap<SubscriptionPlan, SubscriptionPlanSelectModel>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features));

            // CreateModel -> Entity
            CreateMap<SubscriptionPlanCreateModel, SubscriptionPlan>();

            // UpdateModel -> Entity
            CreateMap<SubscriptionPlanUpdateModel, SubscriptionPlan>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

