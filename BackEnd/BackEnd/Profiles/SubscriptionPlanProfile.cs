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
            CreateMap<SubscriptionPlan, SubscriptionPlanSelectModel>();

            // CreateModel -> Entity
            CreateMap<SubscriptionPlanCreateModel, SubscriptionPlan>();

            // UpdateModel -> Entity
            CreateMap<SubscriptionPlanUpdateModel, SubscriptionPlan>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

