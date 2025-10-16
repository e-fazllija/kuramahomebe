using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.SubscriptionFeatureModels;

namespace BackEnd.Profiles
{
    public class SubscriptionFeatureProfile : Profile
    {
        public SubscriptionFeatureProfile()
        {
            // Entity -> SelectModel
            CreateMap<SubscriptionFeature, SubscriptionFeatureSelectModel>();

            // CreateModel -> Entity
            CreateMap<SubscriptionFeatureCreateModel, SubscriptionFeature>();

            // UpdateModel -> Entity
            CreateMap<SubscriptionFeatureUpdateModel, SubscriptionFeature>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

