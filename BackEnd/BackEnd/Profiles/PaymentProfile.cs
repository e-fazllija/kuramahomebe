using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.PaymentModels;

namespace BackEnd.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // Entity -> SelectModel
            CreateMap<Payment, PaymentSelectModel>();

            // CreateModel -> Entity
            CreateMap<PaymentCreateModel, Payment>();

            // UpdateModel -> Entity
            CreateMap<PaymentUpdateModel, Payment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

