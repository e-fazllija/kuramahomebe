using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.StripeWebhookEventModels;

namespace BackEnd.Profiles
{
    public class StripeWebhookEventProfile : Profile
    {
        public StripeWebhookEventProfile()
        {
            // Entity -> SelectModel
            CreateMap<StripeWebhookEvent, StripeWebhookEventSelectModel>();

            // CreateModel -> Entity
            CreateMap<StripeWebhookEventCreateModel, StripeWebhookEvent>();

            // UpdateModel -> Entity
            CreateMap<StripeWebhookEventUpdateModel, StripeWebhookEvent>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

